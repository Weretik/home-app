using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Digests.Planning;
using Notifications.Application.Digests.Texts;
using Notifications.Application.Push;
using Notifications.Application.Sending;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Schedule;

namespace Notifications.Application.Features.Tick.Commands.RunTick;

/// <summary>
/// Plans the due digests on both channels: Telegram for the linked chats (when the bot is configured) and push for
/// the people with a device (when push is configured; spec notifications/002, R-105). Slots from
/// <see cref="DigestSchedule"/>; keys already planned are skipped before anything is read; the rest is read through
/// the other modules' contracts and rendered by <see cref="DigestPlanner"/> once per channel. Each new message is a
/// <see cref="Delivery"/> plus an outbox message, all in one save; a parallel tick that planned the same key first wins
/// (unique key) and this one plans nothing.
/// </summary>
public sealed class RunTickHandler(
    INotificationsDbContext db,
    ITelegramBot bot,
    IWebPush push,
    DigestInputsReader reader,
    NotificationsSettings site,
    TimeProvider clock) : ICommandHandler<RunTickCommand, Result<NotificationsTickReport>>
{
    public async ValueTask<Result<NotificationsTickReport>> Handle(RunTickCommand command, CancellationToken cancellationToken)
    {
        if (!bot.IsConfigured && !push.IsConfigured)
            return new NotificationsTickReport(false, "not_configured", 0);

        var webhook = bot.IsConfigured ? await bot.EnsureWebhookAsync(cancellationToken) : "not_configured";
        var kyivNow = KyivTime.Now(clock);
        var today = DateOnly.FromDateTime(kyivNow);
        var targets = new Dictionary<DeliveryChannel, IReadOnlyDictionary<Audience, long>>
        {
            [DeliveryChannel.Telegram] = bot.IsConfigured
                ? await db.Chats.AsNoTracking().ToDictionaryAsync(c => c.Audience, c => c.ChatId, cancellationToken)
                : new Dictionary<Audience, long>(),
            // Push has no family group: common messages go to every person with a device (R-105).
            [DeliveryChannel.Push] = push.IsConfigured
                ? (await db.PushSubscriptions.AsNoTracking().Select(s => s.Person).Distinct().ToListAsync(cancellationToken))
                    .ToDictionary(p => p, _ => 0L)
                : new Dictionary<Audience, long>(),
        };
        var slots = DigestSchedule.DueAt(kyivNow);
        if (targets.Values.All(t => t.Count == 0) || slots.Count == 0)
            return new NotificationsTickReport(true, webhook, 0);

        var pending = await PendingSlots(slots, targets, cancellationToken);
        var inputs = await reader.ReadAsync(pending, today, cancellationToken);
        var messages = targets
            .SelectMany(t => DigestPlanner.Plan(pending, t.Value, inputs, today, site)
                .Select(m => (Channel: t.Key, Key: Delivery.KeyOf(t.Key, m.Key), Message: m)))
            .ToList();
        var keys = messages.Select(m => m.Key).ToList();
        var existing = await db.Deliveries.AsNoTracking().Where(d => keys.Contains(d.Key)).Select(d => d.Key).ToListAsync(cancellationToken);
        var fresh = messages.Where(m => !existing.Contains(m.Key)).ToList();
        if (fresh.Count == 0)
            return new NotificationsTickReport(true, webhook, 0);

        var now = clock.GetUtcNow();
        foreach (var (channel, key, message) in fresh)
        {
            var delivery = Delivery.Plan(key, channel, message.Kind, message.Audience, message.ChatId, now);
            db.Deliveries.Add(delivery);
            if (channel == DeliveryChannel.Push)
                db.Publish(new PushMessage(delivery.Id, message.Audience, PushTexts.For(message)));
            else
                db.Publish(new TelegramMessage(delivery.Id, message.ChatId, message.Text, message.Buttons));
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A parallel tick planned the same message first; the next tick plans whatever is still missing.
            return new NotificationsTickReport(true, webhook, 0);
        }

        return new NotificationsTickReport(true, webhook, fresh.Count);
    }

    /// <summary>The slots with a message not yet planned on some channel; gift reminders are always read (their keys need the gifts).</summary>
    private async Task<List<DueSlot>> PendingSlots(
        IReadOnlyList<DueSlot> slots,
        Dictionary<DeliveryChannel, IReadOnlyDictionary<Audience, long>> targets,
        CancellationToken cancellationToken)
    {
        var slotKeys = (
            from slot in slots
            where slot.Kind != DigestKind.GiftReminder
            from target in targets
            from audience in DigestRoutes.For(slot.Kind, target.Value)
            select (Slot: slot, Key: Delivery.KeyOf(target.Key, Delivery.KeyOf(slot.Kind, slot.Period, audience)))).ToList();
        var allKeys = slotKeys.Select(x => x.Key).ToList();
        var planned = await db.Deliveries.AsNoTracking().Where(d => allKeys.Contains(d.Key)).Select(d => d.Key).ToListAsync(cancellationToken);
        return [.. slots.Where(s => s.Kind == DigestKind.GiftReminder || slotKeys.Any(x => x.Slot == s && !planned.Contains(x.Key)))];
    }
}

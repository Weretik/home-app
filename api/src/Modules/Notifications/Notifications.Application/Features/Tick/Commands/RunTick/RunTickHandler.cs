using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Digests.Planning;
using Notifications.Application.Sending;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Schedule;

namespace Notifications.Application.Features.Tick.Commands.RunTick;

/// <summary>
/// Plans the due digests: slots from <see cref="DigestSchedule"/>, keys already planned skipped before anything is
/// read, the rest read through the other modules' contracts and rendered by <see cref="DigestPlanner"/>. Each new
/// message is a <see cref="Delivery"/> plus an outbox message, all in one save; a parallel tick that planned the same
/// key first wins (unique key) and this one plans nothing.
/// </summary>
public sealed class RunTickHandler(
    INotificationsDbContext db,
    ITelegramBot bot,
    DigestInputsReader reader,
    NotificationsSettings site,
    TimeProvider clock) : ICommandHandler<RunTickCommand, Result<NotificationsTickReport>>
{
    public async ValueTask<Result<NotificationsTickReport>> Handle(RunTickCommand command, CancellationToken cancellationToken)
    {
        if (!bot.IsConfigured)
            return new NotificationsTickReport(false, "not_configured", 0);

        var webhook = await bot.EnsureWebhookAsync(cancellationToken);
        var kyivNow = KyivTime.Now(clock);
        var today = DateOnly.FromDateTime(kyivNow);
        var chats = await db.Chats.AsNoTracking().ToDictionaryAsync(c => c.Audience, c => c.ChatId, cancellationToken);
        var slots = DigestSchedule.DueAt(kyivNow);
        if (chats.Count == 0 || slots.Count == 0)
            return new NotificationsTickReport(true, webhook, 0);

        // Skip the slots whose messages are all planned already, before reading anything for them.
        var slotKeys = slots
            .Where(s => s.Kind != DigestKind.GiftReminder)
            .SelectMany(s => DigestRoutes.For(s.Kind, chats).Select(a => (Slot: s, Key: Delivery.KeyOf(s.Kind, s.Period, a))))
            .ToList();
        var allKeys = slotKeys.Select(x => x.Key).ToList();
        var planned = await db.Deliveries.AsNoTracking().Where(d => allKeys.Contains(d.Key)).Select(d => d.Key).ToListAsync(cancellationToken);
        var pending = slots
            .Where(s => s.Kind == DigestKind.GiftReminder || slotKeys.Any(x => x.Slot == s && !planned.Contains(x.Key)))
            .ToList();

        var inputs = await reader.ReadAsync(pending, today, cancellationToken);
        var messages = DigestPlanner.Plan(pending, chats, inputs, today, site);
        var keys = messages.Select(m => m.Key).ToList();
        var existing = await db.Deliveries.AsNoTracking().Where(d => keys.Contains(d.Key)).Select(d => d.Key).ToListAsync(cancellationToken);
        var fresh = messages.Where(m => !existing.Contains(m.Key)).ToList();
        if (fresh.Count == 0)
            return new NotificationsTickReport(true, webhook, 0);

        var now = clock.GetUtcNow();
        foreach (var message in fresh)
        {
            var delivery = Delivery.Plan(message.Key, message.Kind, message.Audience, message.ChatId, now);
            db.Deliveries.Add(delivery);
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
}

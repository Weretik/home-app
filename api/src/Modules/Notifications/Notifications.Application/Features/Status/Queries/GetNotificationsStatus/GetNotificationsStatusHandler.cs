using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Chats;
using Notifications.Domain.Schedule;

namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

public sealed class GetNotificationsStatusHandler(INotificationsDbContext db, ITelegramBot bot)
    : IQueryHandler<GetNotificationsStatusQuery, Result<NotificationsStatus>>
{
    private const int DeliveryCount = 10;

    public async ValueTask<Result<NotificationsStatus>> Handle(GetNotificationsStatusQuery query, CancellationToken cancellationToken)
    {
        var linked = await db.Chats.AsNoTracking().ToListAsync(cancellationToken);
        var deliveries = await db.Deliveries.AsNoTracking()
            .OrderByDescending(d => d.CreatedAtUtc)
            .Take(DeliveryCount)
            .Select(d => new NotificationDeliveryItem(d.Kind, d.Audience, d.Key, d.CreatedAtUtc, d.SentAtUtc))
            .ToListAsync(cancellationToken);
        var username = bot.IsConfigured ? await bot.GetUsernameAsync(cancellationToken) : null;

        var chats = Enum.GetValues<Audience>()
            .Select(audience => linked.FirstOrDefault(c => c.Audience == audience) is { } chat
                ? new NotificationChatItem(audience, true, chat.Title, chat.LinkedAtUtc)
                : new NotificationChatItem(audience, false, null, null))
            .ToList();

        return new NotificationsStatus(
            bot.IsConfigured,
            username,
            username is null ? null : $"https://t.me/{username}",
            chats,
            [.. DigestSchedule.Windows.Select(w => new NotificationScheduleItem(w.Kind, w.Title, w.When))],
            deliveries);
    }
}

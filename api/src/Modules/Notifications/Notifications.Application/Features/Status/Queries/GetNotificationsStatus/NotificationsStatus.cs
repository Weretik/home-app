namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>
/// Whether the bot token is set, the bot's @username and link, the three chats (linked or not), the schedule and the
/// last ten planned messages, and when the scheduler last called the tick (silent when that is more than an hour ago or
/// never, so no digest can go out).
/// </summary>
public sealed record NotificationsStatus(
    bool Configured,
    string? BotUsername,
    string? BotLink,
    IReadOnlyList<NotificationChatItem> Chats,
    IReadOnlyList<NotificationScheduleItem> Schedule,
    IReadOnlyList<NotificationDeliveryItem> Deliveries,
    DateTimeOffset? LastTickAtUtc,
    bool SchedulerSilent);

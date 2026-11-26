namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>
/// Whether the bot token is set, the bot's @username and link, the three chats (linked or not), the schedule and the
/// last ten planned messages.
/// </summary>
public sealed record NotificationsStatus(
    bool Configured,
    string? BotUsername,
    string? BotLink,
    IReadOnlyList<NotificationChatItem> Chats,
    IReadOnlyList<NotificationScheduleItem> Schedule,
    IReadOnlyList<NotificationDeliveryItem> Deliveries);

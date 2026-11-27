using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>A planned message on Telegram or push; <see cref="SentAtUtc"/> is null while it waits in the outbox.</summary>
public sealed record NotificationDeliveryItem(
    DigestKind Kind,
    DeliveryChannel Channel,
    Audience Audience,
    string Key,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc);

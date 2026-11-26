using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>A planned message; <see cref="SentAtUtc"/> is null while it waits in the outbox.</summary>
public sealed record NotificationDeliveryItem(
    DigestKind Kind,
    Audience Audience,
    string Key,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SentAtUtc);

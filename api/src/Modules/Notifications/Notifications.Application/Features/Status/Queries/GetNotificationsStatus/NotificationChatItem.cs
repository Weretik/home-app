using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>One of the three chats; <see cref="Title"/> and <see cref="LinkedAtUtc"/> are null while it is not linked.</summary>
public sealed record NotificationChatItem(Audience Audience, bool Linked, string? Title, DateTimeOffset? LinkedAtUtc);

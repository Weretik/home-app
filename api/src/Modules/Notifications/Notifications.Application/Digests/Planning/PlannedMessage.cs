using Notifications.Application.Sending;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace Notifications.Application.Digests.Planning;

/// <summary>One message the tick wants to send, with its unique delivery key.</summary>
public sealed record PlannedMessage(
    string Key,
    DigestKind Kind,
    Audience Audience,
    long ChatId,
    string Text,
    IReadOnlyList<TelegramButton> Buttons);

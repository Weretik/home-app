using Notifications.Application.Sending;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace Notifications.Application.Digests.Planning;

/// <summary>
/// One message the tick wants to send, with its unique delivery key (the Telegram key; push puts <c>push:</c> in front)
/// and the site page it is about (a path such as <c>/todos</c>), where a tap on its push leads.
/// </summary>
public sealed record PlannedMessage(
    string Key,
    DigestKind Kind,
    Audience Audience,
    long ChatId,
    string Text,
    IReadOnlyList<TelegramButton> Buttons,
    string Page);

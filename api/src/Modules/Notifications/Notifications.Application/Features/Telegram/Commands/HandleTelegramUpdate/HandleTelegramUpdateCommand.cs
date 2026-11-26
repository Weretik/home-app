using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

/// <summary>
/// One update from Telegram, reduced to what the bot uses: a text (command), a pressed button, or a change of the
/// bot's membership in a group (spec notifications/001, R-002…R-004, R-015).
/// </summary>
/// <param name="MemberStatus">The bot's new status in the chat (<c>member</c>, <c>administrator</c>, <c>left</c>, <c>kicked</c>).</param>
public sealed record HandleTelegramUpdateCommand(
    long ChatId,
    bool IsGroup,
    string? ChatTitle,
    string? Text,
    string? CallbackId,
    string? CallbackData,
    string? MemberStatus) : ICommand<Result>;

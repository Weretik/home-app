using FluentValidation;

namespace Notifications.Application.Features.Chats.Commands.UnlinkChat;

internal sealed class UnlinkChatValidator : AbstractValidator<UnlinkChatCommand>
{
    public UnlinkChatValidator() => RuleFor(c => c.Audience).IsInEnum();
}

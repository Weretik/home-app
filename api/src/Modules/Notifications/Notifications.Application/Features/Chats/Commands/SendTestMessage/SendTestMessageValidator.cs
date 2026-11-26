using FluentValidation;

namespace Notifications.Application.Features.Chats.Commands.SendTestMessage;

internal sealed class SendTestMessageValidator : AbstractValidator<SendTestMessageCommand>
{
    public SendTestMessageValidator() => RuleFor(c => c.Audience).IsInEnum();
}

using FluentValidation;

namespace Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

internal sealed class HandleTelegramUpdateValidator : AbstractValidator<HandleTelegramUpdateCommand>
{
    public HandleTelegramUpdateValidator()
    {
        RuleFor(c => c.ChatId).NotEqual(0);
        RuleFor(c => c.Text).MaximumLength(4096);
        RuleFor(c => c.CallbackData).MaximumLength(64);
        RuleFor(c => c.ChatTitle).MaximumLength(256);
    }
}

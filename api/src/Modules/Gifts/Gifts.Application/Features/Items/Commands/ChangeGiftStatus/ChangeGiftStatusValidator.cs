using FluentValidation;

namespace Gifts.Application.Features.Items.Commands.ChangeGiftStatus;

internal sealed class ChangeGiftStatusValidator : AbstractValidator<ChangeGiftStatusCommand>
{
    public ChangeGiftStatusValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого подарунка у заголовку If-Match.");
        RuleFor(c => c.Status).IsInEnum();
    }
}

using FluentValidation;

namespace Gifts.Application.Features.Items.Commands.ChangeGiftStatus;

internal sealed class ChangeGiftStatusValidator : AbstractValidator<ChangeGiftStatusCommand>
{
    public ChangeGiftStatusValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the gift you changed in the If-Match header.");
        RuleFor(c => c.Status).IsInEnum();
    }
}

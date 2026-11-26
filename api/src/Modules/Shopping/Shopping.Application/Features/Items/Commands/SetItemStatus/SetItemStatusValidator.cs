using FluentValidation;

namespace Shopping.Application.Features.Items.Commands.SetItemStatus;

internal sealed class SetItemStatusValidator : AbstractValidator<SetItemStatusCommand>
{
    public SetItemStatusValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.By).IsInEnum();
    }
}

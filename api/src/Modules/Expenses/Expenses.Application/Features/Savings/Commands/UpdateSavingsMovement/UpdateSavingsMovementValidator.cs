using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Savings.Commands.UpdateSavingsMovement;

internal sealed class UpdateSavingsMovementValidator : AbstractValidator<UpdateSavingsMovementCommand>
{
    public UpdateSavingsMovementValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого запису у заголовку If-Match.");
        RuleFor(c => c.Person).IsInEnum();
        RuleFor(c => c.Amount).NotEqual(0);
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

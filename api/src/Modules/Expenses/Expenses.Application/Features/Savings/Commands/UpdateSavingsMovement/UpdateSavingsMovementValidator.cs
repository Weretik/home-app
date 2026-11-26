using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Savings.Commands.UpdateSavingsMovement;

internal sealed class UpdateSavingsMovementValidator : AbstractValidator<UpdateSavingsMovementCommand>
{
    public UpdateSavingsMovementValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the entry you edited in the If-Match header.");
        RuleFor(c => c.Person).IsInEnum();
        RuleFor(c => c.Amount).NotEqual(0);
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

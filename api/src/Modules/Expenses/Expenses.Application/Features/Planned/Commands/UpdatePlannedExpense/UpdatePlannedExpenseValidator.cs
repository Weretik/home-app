using Expenses.Domain.Planning;
using FluentValidation;

namespace Expenses.Application.Features.Planned.Commands.UpdatePlannedExpense;

internal sealed class UpdatePlannedExpenseValidator : AbstractValidator<UpdatePlannedExpenseCommand>
{
    public UpdatePlannedExpenseValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the plan you edited in the If-Match header.");
        RuleFor(c => c.Year).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Month).InclusiveBetween(1, 12);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(PlannedExpense.MaxNameLength);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Payer).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
    }
}

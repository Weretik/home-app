using FluentValidation;

namespace Expenses.Application.Features.Planned.Queries.ListPlannedExpenses;

internal sealed class ListPlannedExpensesValidator : AbstractValidator<ListPlannedExpensesQuery>
{
    public ListPlannedExpensesValidator()
    {
        RuleFor(q => q.Year).InclusiveBetween(2000, 2100);
        RuleFor(q => q.Month).InclusiveBetween(1, 12);
    }
}

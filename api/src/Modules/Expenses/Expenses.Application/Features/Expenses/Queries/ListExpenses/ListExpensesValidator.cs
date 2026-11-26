using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

internal sealed class ListExpensesValidator : AbstractValidator<ListExpensesQuery>
{
    public ListExpensesValidator()
    {
        RuleFor(q => q.Year).InclusiveBetween(2000, 2100);
        RuleFor(q => q.Month).InclusiveBetween(1, 12);
        RuleFor(q => q.Payer).IsInEnum();
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

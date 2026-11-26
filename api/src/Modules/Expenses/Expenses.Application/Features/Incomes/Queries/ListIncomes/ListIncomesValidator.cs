using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Expenses.Application.Features.Incomes.Queries.ListIncomes;

internal sealed class ListIncomesValidator : AbstractValidator<ListIncomesQuery>
{
    public ListIncomesValidator()
    {
        RuleFor(q => q.Year).InclusiveBetween(2000, 2100);
        RuleFor(q => q.Month).InclusiveBetween(1, 12);
        RuleFor(q => q.Person).IsInEnum();
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

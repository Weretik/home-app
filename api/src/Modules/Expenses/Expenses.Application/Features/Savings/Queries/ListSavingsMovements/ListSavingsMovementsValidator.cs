using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMovements;

internal sealed class ListSavingsMovementsValidator : AbstractValidator<ListSavingsMovementsQuery>
{
    public ListSavingsMovementsValidator()
    {
        RuleFor(q => q.Year).InclusiveBetween(2000, 2100);
        RuleFor(q => q.Month).InclusiveBetween(1, 12);
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

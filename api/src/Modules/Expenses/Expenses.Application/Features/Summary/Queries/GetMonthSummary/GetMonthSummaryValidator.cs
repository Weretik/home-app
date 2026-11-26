using FluentValidation;

namespace Expenses.Application.Features.Summary.Queries.GetMonthSummary;

internal sealed class GetMonthSummaryValidator : AbstractValidator<GetMonthSummaryQuery>
{
    public GetMonthSummaryValidator()
    {
        RuleFor(q => q.Year).InclusiveBetween(2000, 2100);
        RuleFor(q => q.Month).InclusiveBetween(1, 12);
    }
}

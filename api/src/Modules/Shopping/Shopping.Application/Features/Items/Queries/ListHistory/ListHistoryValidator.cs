using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Shopping.Application.Features.Items.Queries.ListHistory;

internal sealed class ListHistoryValidator : AbstractValidator<ListHistoryQuery>
{
    public ListHistoryValidator()
    {
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Shopping.Application.Features.Keywords.Queries.ListKeywords;

internal sealed class ListKeywordsValidator : AbstractValidator<ListKeywordsQuery>
{
    public ListKeywordsValidator()
    {
        RuleFor(q => q.DepartmentId).NotEqual(Guid.Empty);
        RuleFor(q => q.Search).ValidSearch();
    }
}

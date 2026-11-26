using FluentValidation;
using Shopping.Domain.Parsing;

namespace Shopping.Application.Features.Items.Queries.ListSuggestions;

internal sealed class ListSuggestionsValidator : AbstractValidator<ListSuggestionsQuery>
{
    public ListSuggestionsValidator()
    {
        // The rule is on the query as it is compared: normalised, so "  " is empty and "Ё" counts once.
        RuleFor(q => NameNormalizer.Normalize(q.Query))
            .NotEmpty()
            .MaximumLength(ListSuggestionsQuery.MaxQueryLength)
            .OverridePropertyName(nameof(ListSuggestionsQuery.Query));
        RuleFor(q => q.Limit).InclusiveBetween(1, ListSuggestionsQuery.MaxLimit);
    }
}

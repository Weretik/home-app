using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Queries.ListSuggestions;

/// <summary>
/// Names from every row of the list ever (needed, cart, history) whose normalised form, or one of its words, starts
/// with <see cref="Query"/>: those starting with it first, then the most bought, then the most recently added.
/// </summary>
public sealed record ListSuggestionsQuery(string? Query, int Limit = ListSuggestionsQuery.DefaultLimit)
    : IQuery<Result<IReadOnlyList<NameSuggestion>>>
{
    public const int DefaultLimit = 8;
    public const int MaxLimit = 20;
    public const int MaxQueryLength = 80;
}

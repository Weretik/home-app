using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Queries.ListSuggestions;

namespace Shopping.Api.Endpoints.Items.Reading;

internal static class ListShoppingNameSuggestionsEndpoint
{
    public static RouteGroupBuilder MapListShoppingNameSuggestions(this RouteGroupBuilder group)
    {
        group.MapGet("/items/suggestions", HandleAsync)
            .WithName("listShoppingNameSuggestions")
            .WithSummary(
                "Suggests names ever added to the list whose normalised name, or one of its words, starts with the query: "
                + "names starting with it first, then the most bought, then the most recently added.")
            .Produces<IReadOnlyList<NameSuggestion>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        string? query,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken,
        int limit = ListSuggestionsQuery.DefaultLimit)
    {
        var result = await sender.Send(new ListSuggestionsQuery(query, limit), cancellationToken);
        return result.ToHttpResult(http);
    }
}

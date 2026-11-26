using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Keywords.Queries.ListKeywords;

namespace Shopping.Api.Endpoints.Keywords;

internal static class ListShoppingKeywordsEndpoint
{
    public static RouteGroupBuilder MapListShoppingKeywords(this RouteGroupBuilder group)
    {
        group.MapGet("/keywords", HandleAsync)
            .WithName("listShoppingKeywords")
            .WithSummary("Lists the keywords alphabetically, optionally of one department and containing every word of the search.")
            .Produces<IReadOnlyList<KeywordItem>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid? departmentId, string? search, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListKeywordsQuery(departmentId, search), cancellationToken);
        return result.ToHttpResult(http);
    }
}

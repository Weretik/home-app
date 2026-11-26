using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Queries.ListFrequent;

namespace Shopping.Api.Endpoints.Items.Reading;

internal static class ListFrequentItemsEndpoint
{
    public static RouteGroupBuilder MapListFrequentItems(this RouteGroupBuilder group)
    {
        group.MapGet("/frequent", HandleAsync)
            .WithName("listFrequentItems")
            .WithSummary("Lists the 20 names bought most often in the frequent window, without names that are in the list now.")
            .Produces<IReadOnlyList<FrequentItem>>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListFrequentQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

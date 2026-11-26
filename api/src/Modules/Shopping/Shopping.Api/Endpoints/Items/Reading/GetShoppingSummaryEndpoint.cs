using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Queries.GetSummary;

namespace Shopping.Api.Endpoints.Items.Reading;

internal static class GetShoppingSummaryEndpoint
{
    public static RouteGroupBuilder MapGetShoppingSummary(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", HandleAsync)
            .WithName("getShoppingSummary")
            .WithSummary("The list at a glance for the home page: counts, departments with needed items, next items and top frequent names.")
            .Produces<ShoppingSummary>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSummaryQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

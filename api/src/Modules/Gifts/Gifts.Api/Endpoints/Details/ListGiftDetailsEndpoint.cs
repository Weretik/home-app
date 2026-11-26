using BuildingBlocks.Api.Problems;
using Gifts.Application.Features.Details.Queries.ListDetails;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Details;

internal static class ListGiftDetailsEndpoint
{
    public static RouteGroupBuilder MapListGiftDetails(this RouteGroupBuilder group)
    {
        group.MapGet("/details", HandleAsync)
            .WithName("listGiftDetails")
            .WithSummary("Lists the personal details (sizes and other nuances) by recipient (Maria, Vitaliy, Together) and position.")
            .Produces<IReadOnlyList<GiftDetailItem>>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListDetailsQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

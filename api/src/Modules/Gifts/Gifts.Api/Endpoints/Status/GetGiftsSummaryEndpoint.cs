using BuildingBlocks.Api.Problems;
using Gifts.Application.Features.Summary.Queries.GetGiftsSummary;
using Gifts.Application.Features.Summary.Report;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Status;

internal static class GetGiftsSummaryEndpoint
{
    public static RouteGroupBuilder MapGetGiftsSummary(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", HandleAsync)
            .WithName("getGiftsSummary")
            .WithSummary("The wishlist for the home page: per recipient and in total the wanted and gifted counts and sums, the top High-priority wanted gifts, the upcoming occasions and all details.")
            .Produces<GiftsSummary>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGiftsSummaryQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

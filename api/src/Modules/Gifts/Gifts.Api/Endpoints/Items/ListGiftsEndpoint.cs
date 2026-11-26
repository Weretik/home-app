using BuildingBlocks.Api.Problems;
using Gifts.Application.Features.Items.Queries.ListGifts;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Items;

internal static class ListGiftsEndpoint
{
    public static RouteGroupBuilder MapListGifts(this RouteGroupBuilder group)
    {
        group.MapGet("/items", HandleAsync)
            .WithName("listGifts")
            .WithSummary("Lists the wanted gifts (archived=false, by priority, then the nearest occasion) or the archive of gifted and dropped ones (archived=true, latest gifted first), with their variants.")
            .Produces<IReadOnlyList<GiftItem>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        ISender sender, HttpContext http, CancellationToken cancellationToken, bool archived = false)
    {
        var result = await sender.Send(new ListGiftsQuery(archived), cancellationToken);
        return result.ToHttpResult(http);
    }
}

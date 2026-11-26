using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Items.Commands.ChangeGiftStatus;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Status;

internal static class ChangeGiftStatusEndpoint
{
    public static RouteGroupBuilder MapChangeGiftStatus(this RouteGroupBuilder group)
    {
        group.MapPut("/items/{id:guid}/status", HandleAsync)
            .WithName("changeGiftStatus")
            .WithSummary("Marks a gift Gifted (with giftedOn, today in Kyiv by default) or Dropped, which moves it to the archive, or Wanted, which returns it to the list. Send its version in If-Match.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        [FromHeader(Name = GiftVersionHeader.Name)] string? ifMatch,
        GiftStatusRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeGiftStatusCommand(id, GiftVersionHeader.Parse(ifMatch), request.Status, request.GiftedOn), cancellationToken);
        return result.ToHttpResult(http);
    }
}

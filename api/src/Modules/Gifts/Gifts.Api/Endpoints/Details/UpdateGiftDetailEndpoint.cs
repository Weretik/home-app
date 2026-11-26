using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Details.Commands.UpdateDetail;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Details;

internal static class UpdateGiftDetailEndpoint
{
    public static RouteGroupBuilder MapUpdateGiftDetail(this RouteGroupBuilder group)
    {
        group.MapPut("/details/{id:guid}", HandleAsync)
            .WithName("updateGiftDetail")
            .WithSummary("Changes a detail; its position stays. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        GiftDetailRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateDetailCommand(id, GiftVersionHeader.Parse(ifMatch), request.Recipient, request.Label, request.Value), cancellationToken);
        return result.ToHttpResult(http);
    }
}

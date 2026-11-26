using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Items.Commands.UpdateGift;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Items;

internal static class UpdateGiftEndpoint
{
    public static RouteGroupBuilder MapUpdateGift(this RouteGroupBuilder group)
    {
        group.MapPut("/items/{id:guid}", HandleAsync)
            .WithName("updateGift")
            .WithSummary("Changes a gift and replaces its whole variant list; the status stays. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        GiftRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateGiftCommand(
                id,
                GiftVersionHeader.Parse(ifMatch),
                request.Recipient,
                request.Name,
                request.CategoryId,
                request.Priority,
                request.Price,
                request.Link,
                request.Occasion,
                request.OccasionDate,
                request.Notes,
                request.VariantDrafts()),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

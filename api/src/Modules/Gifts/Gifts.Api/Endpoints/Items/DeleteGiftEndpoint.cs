using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Items.Commands.DeleteGift;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Items;

internal static class DeleteGiftEndpoint
{
    public static RouteGroupBuilder MapDeleteGift(this RouteGroupBuilder group)
    {
        group.MapDelete("/items/{id:guid}", HandleAsync)
            .WithName("deleteGift")
            .WithSummary("Deletes a gift and its variants for good.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteGiftCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

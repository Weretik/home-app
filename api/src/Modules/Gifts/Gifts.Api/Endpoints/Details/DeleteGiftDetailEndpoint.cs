using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Details.Commands.DeleteDetail;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Details;

internal static class DeleteGiftDetailEndpoint
{
    public static RouteGroupBuilder MapDeleteGiftDetail(this RouteGroupBuilder group)
    {
        group.MapDelete("/details/{id:guid}", HandleAsync)
            .WithName("deleteGiftDetail")
            .WithSummary("Deletes a detail for good.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteDetailCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

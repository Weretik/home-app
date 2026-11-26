using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Savings.Commands.DeleteSavingsMovement;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Savings;

internal static class DeleteSavingsMovementEndpoint
{
    public static RouteGroupBuilder MapDeleteSavingsMovement(this RouteGroupBuilder group)
    {
        group.MapDelete("/savings/{id:guid}", HandleAsync)
            .WithName("deleteSavingsMovement")
            .WithSummary("Deletes a savings movement for good.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSavingsMovementCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Budget.Commands.DeleteSpendingLimit;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Budget.Limits;

internal static class DeleteSpendingLimitEndpoint
{
    public static RouteGroupBuilder MapDeleteSpendingLimit(this RouteGroupBuilder group)
    {
        group.MapDelete("/budget/limits/{id:guid}", HandleAsync)
            .WithName("deleteSpendingLimit")
            .WithSummary("Deletes a spending limit for good; its expenses stay.")
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
        var result = await sender.Send(new DeleteSpendingLimitCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Planned.Commands.DeletePlannedExpense;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Planned.Plans;

internal static class DeletePlannedExpenseEndpoint
{
    public static RouteGroupBuilder MapDeletePlannedExpense(this RouteGroupBuilder group)
    {
        group.MapDelete("/planned/{id:guid}", HandleAsync)
            .WithName("deletePlannedExpense")
            .WithSummary("Drops a planned expense for good; nothing is recorded.")
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
        var result = await sender.Send(new DeletePlannedExpenseCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Planned.Commands.UpdatePlannedExpense;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Planned.Plans;

internal static class UpdatePlannedExpenseEndpoint
{
    public static RouteGroupBuilder MapUpdatePlannedExpense(this RouteGroupBuilder group)
    {
        group.MapPut("/planned/{id:guid}", HandleAsync)
            .WithName("updatePlannedExpense")
            .WithSummary("Changes a planned expense. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        [FromHeader(Name = EntryVersionHeader.Name)] string? ifMatch,
        PlannedExpenseRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdatePlannedExpenseCommand(id, EntryVersionHeader.Parse(ifMatch), request.Year, request.Month, request.Name, request.CategoryId, request.Payer, request.Amount),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Planned.Commands.CreatePlannedExpense;
using Expenses.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Planned.Plans;

internal static class CreatePlannedExpenseEndpoint
{
    public static RouteGroupBuilder MapCreatePlannedExpense(this RouteGroupBuilder group)
    {
        group.MapPost("/planned", HandleAsync)
            .WithName("createPlannedExpense")
            .WithSummary("Plans an expense for a month. Send a new Idempotency-Key per plan; a repeat returns the first id.")
            .Produces<CreatedEntry>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        PlannedExpenseRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreatePlannedExpenseCommand(idempotencyKey ?? string.Empty, request.Year, request.Month, request.Name, request.CategoryId, request.Payer, request.Amount),
            cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/expenses/planned/{created.Id}");
    }
}

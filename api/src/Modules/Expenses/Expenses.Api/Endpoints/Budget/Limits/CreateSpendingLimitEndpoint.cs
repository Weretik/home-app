using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;
using Expenses.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Budget.Limits;

internal static class CreateSpendingLimitEndpoint
{
    public static RouteGroupBuilder MapCreateSpendingLimit(this RouteGroupBuilder group)
    {
        group.MapPost("/budget/limits", HandleAsync)
            .WithName("createSpendingLimit")
            .WithSummary("Adds a spending limit at the end of the list. Send a new Idempotency-Key per limit.")
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
        SpendingLimitRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateSpendingLimitCommand(idempotencyKey ?? string.Empty, request.Name, request.Period, request.Amount, request.CategoryIds ?? []), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/expenses/budget/limits/{created.Id}");
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Savings.Commands.CreateSavingsMovement;
using Expenses.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Savings;

internal static class CreateSavingsMovementEndpoint
{
    public static RouteGroupBuilder MapCreateSavingsMovement(this RouteGroupBuilder group)
    {
        group.MapPost("/savings", HandleAsync)
            .WithName("createSavingsMovement")
            .WithSummary("Adds a savings movement. Send a new Idempotency-Key per entry; a repeat returns the first id.")
            .Produces<CreatedEntry>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        SavingsMovementRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateSavingsMovementCommand(idempotencyKey ?? string.Empty, request.Date, request.Person, request.Amount, request.Comment), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/expenses/savings/{created.Id}");
    }
}

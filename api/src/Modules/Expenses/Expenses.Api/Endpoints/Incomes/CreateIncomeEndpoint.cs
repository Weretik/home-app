using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Incomes.Commands.CreateIncome;
using Expenses.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Incomes;

internal static class CreateIncomeEndpoint
{
    public static RouteGroupBuilder MapCreateIncome(this RouteGroupBuilder group)
    {
        group.MapPost("/incomes", HandleAsync)
            .WithName("createIncome")
            .WithSummary("Adds an income. Send a new Idempotency-Key per entry; a repeat returns the first id.")
            .Produces<CreatedEntry>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        IncomeRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateIncomeCommand(idempotencyKey ?? string.Empty, request.Date, request.Person, request.Amount, request.Comment), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/expenses/incomes/{created.Id}");
    }
}

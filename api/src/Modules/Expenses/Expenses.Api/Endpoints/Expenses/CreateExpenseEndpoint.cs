using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Expenses.Commands.CreateExpense;
using Expenses.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Expenses;

internal static class CreateExpenseEndpoint
{
    public static RouteGroupBuilder MapCreateExpense(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .WithName("createExpense")
            .WithSummary("Adds an expense. Send a new Idempotency-Key per entry; a repeat returns the first id.")
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
        ExpenseRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateExpenseCommand(idempotencyKey ?? string.Empty, request.Date, request.Payer, request.CategoryId, request.Amount, request.PaymentMethod, request.Comment), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/expenses/{created.Id}");
    }
}

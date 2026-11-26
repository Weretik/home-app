using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Expenses.Commands.UpdateExpense;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Expenses;

internal static class UpdateExpenseEndpoint
{
    public static RouteGroupBuilder MapUpdateExpense(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", HandleAsync)
            .WithName("updateExpense")
            .WithSummary("Changes an expense. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        ExpenseRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateExpenseCommand(id, EntryVersionHeader.Parse(ifMatch), request.Date, request.Payer, request.CategoryId, request.Amount, request.PaymentMethod, request.Comment), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Planned.Commands.RecordPlannedExpense;
using Expenses.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Planned.Recording;

internal static class RecordPlannedExpenseEndpoint
{
    public static RouteGroupBuilder MapRecordPlannedExpense(this RouteGroupBuilder group)
    {
        group.MapPost("/planned/{id:guid}/record", HandleAsync)
            .WithName("recordPlannedExpense")
            .WithSummary("Records a planned expense as a real one (counted in the limits) and drops the plan, in one transaction. Send the plan's version in If-Match; returns the new expense id.")
            .Produces<CreatedEntry>(StatusCodes.Status201Created)
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
        RecordPlannedExpenseRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RecordPlannedExpenseCommand(id, EntryVersionHeader.Parse(ifMatch), request.Date, request.PaymentMethod, request.Comment),
            cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/expenses/{created.Id}");
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Budget.Limits;

internal static class UpdateSpendingLimitEndpoint
{
    public static RouteGroupBuilder MapUpdateSpendingLimit(this RouteGroupBuilder group)
    {
        group.MapPut("/budget/limits/{id:guid}", HandleAsync)
            .WithName("updateSpendingLimit")
            .WithSummary("Changes a spending limit; the amount applies from the period of amountFrom on (left out: the current one), earlier periods keep theirs. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        SpendingLimitRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSpendingLimitCommand(id, EntryVersionHeader.Parse(ifMatch), request.Name, request.Period, request.Amount, request.CategoryIds ?? [], request.AmountFrom), cancellationToken);
        return result.ToHttpResult(http);
    }
}

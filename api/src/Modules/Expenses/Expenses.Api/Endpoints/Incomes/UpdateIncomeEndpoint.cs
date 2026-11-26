using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Incomes.Commands.UpdateIncome;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Incomes;

internal static class UpdateIncomeEndpoint
{
    public static RouteGroupBuilder MapUpdateIncome(this RouteGroupBuilder group)
    {
        group.MapPut("/incomes/{id:guid}", HandleAsync)
            .WithName("updateIncome")
            .WithSummary("Changes an income. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        IncomeRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateIncomeCommand(id, EntryVersionHeader.Parse(ifMatch), request.Date, request.Person, request.Amount, request.Comment), cancellationToken);
        return result.ToHttpResult(http);
    }
}

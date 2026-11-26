using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Incomes.Commands.DeleteIncome;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Incomes;

internal static class DeleteIncomeEndpoint
{
    public static RouteGroupBuilder MapDeleteIncome(this RouteGroupBuilder group)
    {
        group.MapDelete("/incomes/{id:guid}", HandleAsync)
            .WithName("deleteIncome")
            .WithSummary("Deletes an income for good.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteIncomeCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Budget.Queries.GetBudget;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Budget.Overview;

internal static class GetBudgetEndpoint
{
    public static RouteGroupBuilder MapGetBudget(this RouteGroupBuilder group)
    {
        group.MapGet("/budget", HandleAsync)
            .WithName("getBudget")
            .WithSummary("The budget settings (monthly limit, savings goal, limits start day) and every spending limit, in order.")
            .Produces<BudgetView>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBudgetQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

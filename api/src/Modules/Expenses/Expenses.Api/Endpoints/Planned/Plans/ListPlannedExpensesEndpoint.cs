using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Planned.Queries.ListPlannedExpenses;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Planned.Plans;

internal static class ListPlannedExpensesEndpoint
{
    public static RouteGroupBuilder MapListPlannedExpenses(this RouteGroupBuilder group)
    {
        group.MapGet("/planned", HandleAsync)
            .WithName("listPlannedExpenses")
            .WithSummary("Lists the planned expenses of one month by name, with their total.")
            .Produces<PlannedExpenseList>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int year, int month, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPlannedExpensesQuery(year, month), cancellationToken);
        return result.ToHttpResult(http);
    }
}

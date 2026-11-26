using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Summary.Queries.GetMonthSummary;
using Expenses.Application.Features.Summary.Report;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Budget.Overview;

internal static class GetMonthSummaryEndpoint
{
    public static RouteGroupBuilder MapGetMonthSummary(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", HandleAsync)
            .WithName("getMonthSummary")
            .WithSummary("One month at a glance: incomes, expenses, the month's result, the savings jar, limits and top categories.")
            .Produces<MonthSummary>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int year, int month, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMonthSummaryQuery(year, month), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Savings.Queries.ListSavingsMonths;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Savings;

internal static class ListSavingsMonthsEndpoint
{
    public static RouteGroupBuilder MapListSavingsMonths(this RouteGroupBuilder group)
    {
        group.MapGet("/savings/months", HandleAsync)
            .WithName("listSavingsMonths")
            .WithSummary("The current savings balance and every month's deposits, withdrawals and closing balance.")
            .Produces<SavingsOverview>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListSavingsMonthsQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

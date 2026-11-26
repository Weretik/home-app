using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Savings.Queries.ListSavingsMovements;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Savings;

internal static class ListSavingsMovementsEndpoint
{
    public static RouteGroupBuilder MapListSavingsMovements(this RouteGroupBuilder group)
    {
        group.MapGet("/savings", HandleAsync)
            .WithName("listSavingsMovements")
            .WithSummary("Lists one month's savings movements, newest first, with the month's sums and balances.")
            .Produces<SavingsMovementList>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int year,
        int month,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 50)
    {
        var result = await sender.Send(new ListSavingsMovementsQuery(year, month, page, pageSize), cancellationToken);
        return result.ToHttpResult(http);
    }
}

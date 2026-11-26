using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Incomes.Queries.ListIncomes;
using Expenses.Domain.Parties;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Ledger;

internal static class ListIncomesEndpoint
{
    public static RouteGroupBuilder MapListIncomes(this RouteGroupBuilder group)
    {
        group.MapGet("/incomes", HandleAsync)
            .WithName("listIncomes")
            .WithSummary("Lists one month's incomes, newest first, with the month's total.")
            .Produces<IncomeList>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int year,
        int month,
        Person? person,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 50)
    {
        var result = await sender.Send(new ListIncomesQuery(year, month, person, page, pageSize), cancellationToken);
        return result.ToHttpResult(http);
    }
}

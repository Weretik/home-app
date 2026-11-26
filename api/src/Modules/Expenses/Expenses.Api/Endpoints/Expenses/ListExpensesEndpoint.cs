using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Expenses.Queries.ListExpenses;
using Expenses.Domain.Parties;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Expenses;

internal static class ListExpensesEndpoint
{
    public static RouteGroupBuilder MapListExpenses(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .WithName("listExpenses")
            .WithSummary("Lists the expenses of one month (year, month) or of a period (from, to, both included), newest first, with their total.")
            .Produces<ExpenseList>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int? year,
        int? month,
        DateOnly? from,
        DateOnly? to,
        Guid? categoryId,
        Payer? payer,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 50)
    {
        var result = await sender.Send(
            new ListExpensesQuery(year, month, from, to, categoryId, payer, page, pageSize), cancellationToken);
        return result.ToHttpResult(http);
    }
}

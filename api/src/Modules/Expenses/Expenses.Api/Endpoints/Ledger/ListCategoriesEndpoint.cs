using BuildingBlocks.Api.Problems;
using Expenses.Application.Features.Categories.Queries.ListCategories;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Ledger;

internal static class ListCategoriesEndpoint
{
    public static RouteGroupBuilder MapListCategories(this RouteGroupBuilder group)
    {
        group.MapGet("/categories", HandleAsync)
            .WithName("listExpenseCategories")
            .WithSummary("Lists the expense categories in display order.")
            .Produces<IReadOnlyList<CategoryItem>>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListCategoriesQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

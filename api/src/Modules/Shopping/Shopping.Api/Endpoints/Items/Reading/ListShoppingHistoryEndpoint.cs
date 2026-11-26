using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Queries.ListHistory;

namespace Shopping.Api.Endpoints.Items.Reading;

internal static class ListShoppingHistoryEndpoint
{
    public static RouteGroupBuilder MapListShoppingHistory(this RouteGroupBuilder group)
    {
        group.MapGet("/history", HandleAsync)
            .WithName("listShoppingHistory")
            .WithSummary("Lists bought items that left the list, newest bought first, one page at a time.")
            .Produces<ShoppingHistoryPage>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        ISender sender, HttpContext http, CancellationToken cancellationToken, int page = 1, int pageSize = 50)
    {
        var result = await sender.Send(new ListHistoryQuery(page, pageSize), cancellationToken);
        return result.ToHttpResult(http);
    }
}

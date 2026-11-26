using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Queries.ListItems;

namespace Shopping.Api.Endpoints.Items.Reading;

internal static class ListShoppingItemsEndpoint
{
    public static RouteGroupBuilder MapListShoppingItems(this RouteGroupBuilder group)
    {
        group.MapGet("/items", HandleAsync)
            .WithName("listShoppingItems")
            .WithSummary("Lists what is needed (by department in store order, urgent first) and what is in the cart (latest bought first).")
            .Produces<ShoppingList>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListItemsQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

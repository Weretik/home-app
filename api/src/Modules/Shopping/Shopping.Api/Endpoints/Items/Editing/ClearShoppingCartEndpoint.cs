using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Commands.ClearCart;

namespace Shopping.Api.Endpoints.Items.Editing;

internal static class ClearShoppingCartEndpoint
{
    public static RouteGroupBuilder MapClearShoppingCart(this RouteGroupBuilder group)
    {
        group.MapPost("/items/clear-cart", HandleAsync)
            .WithName("clearShoppingCart")
            .WithSummary("«Убрать купленные»: moves everything in the cart to history now and says how many items left.")
            .Produces<ClearCartResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ClearCartCommand(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

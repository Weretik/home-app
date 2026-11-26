using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Settings.Queries.GetSettings;

namespace Shopping.Api.Endpoints.Settings;

internal static class GetShoppingSettingsEndpoint
{
    public static RouteGroupBuilder MapGetShoppingSettings(this RouteGroupBuilder group)
    {
        group.MapGet("/settings", HandleAsync)
            .WithName("getShoppingSettings")
            .WithSummary("The cart hours and the frequent window, with the version to edit them with.")
            .Produces<ShoppingSettingsView>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSettingsQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

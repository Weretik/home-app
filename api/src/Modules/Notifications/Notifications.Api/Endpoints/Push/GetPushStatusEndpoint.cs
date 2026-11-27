using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Push.Queries.GetPushStatus;

namespace Notifications.Api.Endpoints.Push;

internal static class GetPushStatusEndpoint
{
    public static RouteGroupBuilder MapGetPushStatus(this RouteGroupBuilder group)
    {
        group.MapGet("/push", HandleAsync)
            .WithName("getPushStatus")
            .WithSummary("Web Push for the signed-in person: whether it is configured, the VAPID public key to subscribe with, and the person's devices.")
            .Produces<PushStatus>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPushStatusQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

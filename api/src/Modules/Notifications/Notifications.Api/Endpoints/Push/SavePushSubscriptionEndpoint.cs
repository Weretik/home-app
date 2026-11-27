using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Push.Commands.SavePushSubscription;
using Notifications.Application.Features.Push.Queries.GetPushStatus;

namespace Notifications.Api.Endpoints.Push;

internal static class SavePushSubscriptionEndpoint
{
    public static RouteGroupBuilder MapSavePushSubscription(this RouteGroupBuilder group)
    {
        group.MapPut("/push/subscriptions", HandleAsync)
            .WithName("savePushSubscription")
            .WithSummary("Saves this browser's push subscription for the signed-in person; saving the same endpoint again updates it. 409 when push is not configured or the member is not Vitaliy or Maria.")
            .Produces<PushDevice>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        SavePushSubscriptionRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var command = new SavePushSubscriptionCommand(request.Endpoint, request.P256dh, request.Auth, request.Device);
        var result = await sender.Send(command, cancellationToken);
        return result.ToHttpResult(http);
    }
}

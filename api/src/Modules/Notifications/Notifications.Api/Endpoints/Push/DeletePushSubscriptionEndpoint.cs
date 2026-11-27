using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Push.Commands.DeletePushSubscription;

namespace Notifications.Api.Endpoints.Push;

internal static class DeletePushSubscriptionEndpoint
{
    public static RouteGroupBuilder MapDeletePushSubscription(this RouteGroupBuilder group)
    {
        group.MapDelete("/push/subscriptions/{id:guid}", HandleAsync)
            .WithName("deletePushSubscription")
            .WithSummary("Removes one of the signed-in person's push devices. 404 when it is not one of theirs.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeletePushSubscriptionCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

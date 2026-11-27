using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Push.Commands.SendPushTest;

namespace Notifications.Api.Endpoints.Push;

internal static class SendPushTestEndpoint
{
    public static RouteGroupBuilder MapSendPushTest(this RouteGroupBuilder group)
    {
        group.MapPost("/push/test", HandleAsync)
            .WithName("sendPushTest")
            .WithSummary("Pushes a test message to every device of the signed-in person at once and says how many accepted it. 404 without devices, 409 when push is not configured or every device refused.")
            .Produces<PushTestResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SendPushTestCommand(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Chats.Commands.UnlinkChat;
using Notifications.Domain.Chats;

namespace Notifications.Api.Endpoints.Site;

internal static class UnlinkNotificationsChatEndpoint
{
    public static RouteGroupBuilder MapUnlinkNotificationsChat(this RouteGroupBuilder group)
    {
        group.MapDelete("/chats/{audience}", HandleAsync)
            .WithName("unlinkNotificationsChat")
            .WithSummary("Unlinks the Telegram chat of Vitaliy, Maria or Family, so another chat can be linked with /start. 404 when none is linked.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(Audience audience, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UnlinkChatCommand(audience), cancellationToken);
        return result.ToHttpResult(http);
    }
}

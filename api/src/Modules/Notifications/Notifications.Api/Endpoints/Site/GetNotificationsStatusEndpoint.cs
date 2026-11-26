using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

namespace Notifications.Api.Endpoints.Site;

internal static class GetNotificationsStatusEndpoint
{
    public static RouteGroupBuilder MapGetNotificationsStatus(this RouteGroupBuilder group)
    {
        group.MapGet("/status", HandleAsync)
            .WithName("getNotificationsStatus")
            .WithSummary("The Telegram bot: whether its token is set, its @username and link, the three chats (Vitaliy, Maria, Family) linked or not, the schedule in Kyiv time and the last ten planned messages.")
            .Produces<NotificationsStatus>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNotificationsStatusQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

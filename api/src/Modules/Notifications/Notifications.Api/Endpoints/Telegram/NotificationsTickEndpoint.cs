using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.Scheduling;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Notifications.Application.Features.Tick.Commands.RunTick;

namespace Notifications.Api.Endpoints.Telegram;

/// <summary>
/// <c>POST /internal/notifications/tick</c>: the scheduler (GitHub Actions cron, ADR 0034) calls it every 15 minutes
/// with the scheduler key, before the outbox tick that sends what it planned. Hidden from OpenAPI.
/// </summary>
internal static class NotificationsTickEndpoint
{
    public static IEndpointRouteBuilder MapNotificationsTick(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/internal/notifications/tick", HandleAsync).ExcludeFromDescription().AllowAnonymous();
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext http, IOptions<OutboxTickOptions> options, ISender sender, CancellationToken cancellationToken) =>
        SchedulerKey.Refusal(http, options.Value.Key)
            ?? (await sender.Send(new RunTickCommand(), cancellationToken)).ToHttpResult(http);
}

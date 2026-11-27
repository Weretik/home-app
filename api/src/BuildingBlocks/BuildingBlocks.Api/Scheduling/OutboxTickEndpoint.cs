using BuildingBlocks.Application.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Api.Scheduling;

/// <summary>
/// <c>POST /internal/outbox/tick</c>: the scheduler (a GitHub Actions cron, ADR 0034) calls it every few minutes, and
/// each call sends the due outbox messages of all modules. No background thread is needed, so the API can sleep.
/// </summary>
public static class OutboxTickEndpoint
{
    public const string HeaderName = SchedulerKey.HeaderName;

    public static IEndpointRouteBuilder MapOutboxTick(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/internal/outbox/tick", RunAsync).ExcludeFromDescription().AllowAnonymous();
        return endpoints;
    }

    private static async Task<IResult> RunAsync(
        HttpContext httpContext,
        IOptions<OutboxTickOptions> options,
        IOutboxTick tick,
        CancellationToken cancellationToken) =>
        SchedulerKey.Refusal(httpContext, options.Value.Key) ?? TypedResults.Ok(await tick.RunAsync(cancellationToken));
}

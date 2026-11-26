using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Application.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Api.Scheduling;

/// <summary>
/// <c>POST /internal/outbox/tick</c>: Cloud Scheduler calls it every few minutes, and each call sends the due
/// outbox messages of all modules. No background thread is needed, so the API can scale to zero.
/// </summary>
public static class OutboxTickEndpoint
{
    public const string HeaderName = "X-Scheduler-Key";

    public static IEndpointRouteBuilder MapOutboxTick(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/internal/outbox/tick", RunAsync).ExcludeFromDescription();
        return endpoints;
    }

    private static async Task<IResult> RunAsync(
        HttpContext httpContext,
        IOptions<OutboxTickOptions> options,
        IOutboxTick tick,
        CancellationToken cancellationToken)
    {
        var expected = options.Value.Key;
        if (string.IsNullOrEmpty(expected))
            return TypedResults.NotFound();

        var actual = httpContext.Request.Headers[HeaderName].ToString();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected)))
            return TypedResults.Unauthorized();

        return TypedResults.Ok(await tick.RunAsync(cancellationToken));
    }
}

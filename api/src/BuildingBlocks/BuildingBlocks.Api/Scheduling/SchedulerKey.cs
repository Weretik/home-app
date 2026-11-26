using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Api.Scheduling;

/// <summary>
/// The shared key of the scheduler endpoints (<c>X-Scheduler-Key</c> = <c>OutboxTick:Key</c>), compared in constant
/// time. Without a configured key the endpoints do not exist (404).
/// </summary>
public static class SchedulerKey
{
    public const string HeaderName = "X-Scheduler-Key";

    /// <summary>Null when the request may run, else the response to send.</summary>
    public static IResult? Refusal(HttpContext httpContext, string? expected)
    {
        if (string.IsNullOrEmpty(expected))
            return TypedResults.NotFound();

        var actual = httpContext.Request.Headers[HeaderName].ToString();
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected))
            ? null
            : TypedResults.Unauthorized();
    }
}

using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Api.Telemetry;

/// <summary>
/// Requests not worth a trace: <c>/health</c> is pinged every few minutes by the availability test and the deploy,
/// carries no family data, and its result is recorded by the availability test itself. Leaving it out keeps the
/// daily volume far under the free allowance.
/// </summary>
public static class QuietPaths
{
    public static bool ShouldTrace(HttpContext context) => !context.Request.Path.StartsWithSegments("/health");
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Api.Health;

/// <summary>
/// Two views of the same health checks: <c>GET /health</c> for probes and the deploy smoke test, and
/// <c>GET /api/system/status</c>, a documented operation the web app reads through the typed client. Both are
/// anonymous: they hold no family data and the deploy checks call them (ADR 0037).
/// </summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = (context, report) => context.Response.WriteAsJsonAsync(
                SystemStatus.From(report, context.RequestServices.GetRequiredService<IHostEnvironment>())),
        }).AllowAnonymous();

        endpoints.MapGet("/api/system/status", GetStatusAsync)
            .WithName("getSystemStatus")
            .WithSummary("Reports whether the API and its dependencies are healthy.")
            .WithTags("System")
            .Produces<SystemStatus>(StatusCodes.Status200OK)
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<Ok<SystemStatus>> GetStatusAsync(
        HealthCheckService healthChecks, IHostEnvironment environment, CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        return TypedResults.Ok(SystemStatus.From(report, environment));
    }
}

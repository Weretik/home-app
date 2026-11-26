using System.Reflection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HomeApp.Api.Platform;

internal static class HealthSetup
{
    /// <summary>The database check is added only when <c>ConnectionStrings:Default</c> is set.</summary>
    public static IServiceCollection AddHealth(this IServiceCollection services, IConfiguration configuration)
    {
        var checks = services.AddHealthChecks();
        var connectionString = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connectionString))
            checks.Add(new HealthCheckRegistration("database", _ => new DatabaseHealthCheck(connectionString), null, null));
        return services;
    }

    /// <summary><c>GET /health</c> answers with JSON the web app shows on its start page.</summary>
    public static void MapHealth(this WebApplication app) =>
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteJson });

    private static Task WriteJson(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new HealthResponse(
            report.Status.ToString(),
            Version,
            context.RequestServices.GetRequiredService<IHostEnvironment>().EnvironmentName,
            report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString())));

    private static readonly string Version =
        typeof(HealthSetup).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";
}

internal sealed record HealthResponse(
    string Status,
    string Version,
    string Environment,
    IReadOnlyDictionary<string, string> Checks);

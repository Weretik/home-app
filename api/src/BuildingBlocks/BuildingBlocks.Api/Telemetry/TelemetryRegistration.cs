using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace BuildingBlocks.Api.Telemetry;

/// <summary>
/// Requests, dependencies (database, Telegram, Google, push services), exceptions and logs go to Application Insights
/// through OpenTelemetry (ADR 0044). Turned on only where the App Service sets the connection string, so local runs,
/// tests and CI send nothing.
/// </summary>
public static class TelemetryRegistration
{
    /// <summary>The setting Azure writes when Application Insights is attached to the App Service.</summary>
    public const string ConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration[ConnectionStringKey]))
            return services;

        services.AddOpenTelemetry().UseAzureMonitor();
        services.Configure<AspNetCoreTraceInstrumentationOptions>(options => options.Filter = QuietPaths.ShouldTrace);
        return services;
    }
}

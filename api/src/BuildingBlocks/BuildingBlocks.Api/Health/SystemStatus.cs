using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Api.Health;

/// <summary>The overall status, the build version with its commit, the environment, and the status of each check.</summary>
public sealed record SystemStatus(
    HealthStatus Status,
    string Version,
    string Environment,
    IReadOnlyDictionary<string, HealthStatus> Checks)
{
    private static readonly string BuildVersion =
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "unknown";

    public static SystemStatus From(HealthReport report, IHostEnvironment environment) =>
        new(
            report.Status,
            BuildVersion,
            environment.EnvironmentName,
            report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status));
}

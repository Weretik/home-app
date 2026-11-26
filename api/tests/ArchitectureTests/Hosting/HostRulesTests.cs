using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace ArchitectureTests.Hosting;

/// <summary>
/// The host only composes modules, checks its service registrations in every environment, and runs no work in the
/// background: scheduled work is an HTTP endpoint called by an external scheduler (ADR 0006).
/// </summary>
public sealed class HostRulesTests
{
    /// <summary>Hosted services that are not background work, with the reason.</summary>
    private static readonly Dictionary<string, string> StartupSteps = new()
    {
        ["Host.Api.Composition.DatabaseStartup"] = "local development only: migrates once in StartAsync, then does nothing",
    };

    private static readonly string Program = File.ReadAllText(Path.Combine(Repository.Root, "src/Host/Host.Api/Program.cs"));

    private static IEnumerable<Assembly> BackendAssemblies() =>
        new[] { "Host.Api", "BuildingBlocks.Domain", "BuildingBlocks.Application", "BuildingBlocks.Infrastructure", "BuildingBlocks.Api" }
            .Concat(Repository.Modules.SelectMany(module => Repository.Layers.Select(layer => $"{module}.{layer}")))
            .Select(Repository.Assembly);

    [Fact]
    public void The_Host_ValidatesScopesAndTheBuild_InEveryEnvironment()
    {
        Assert.Contains("ValidateScopes = true", Program, StringComparison.Ordinal);
        Assert.Contains("ValidateOnBuild = true", Program, StringComparison.Ordinal);
        Assert.DoesNotContain("IsDevelopment()", Program, StringComparison.Ordinal);
    }

    [Fact]
    public void The_Host_AddsAndMapsEveryModule_ThroughItsOneEntryPoint()
    {
        var missing = Repository.Modules
            .SelectMany(module => new[] { $"Add{module}Module(", $"Map{module}Module(" })
            .Where(call => !Program.Contains(call, StringComparison.Ordinal));

        Assert.Empty(missing);
    }

    [Fact]
    public void Nothing_RunsInTheBackground()
    {
        var violations = BackendAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && typeof(IHostedService).IsAssignableFrom(type))
            .Where(type => typeof(BackgroundService).IsAssignableFrom(type) || !StartupSteps.ContainsKey(type.FullName!))
            .Select(type => type.FullName);

        Assert.Empty(violations);
    }
}

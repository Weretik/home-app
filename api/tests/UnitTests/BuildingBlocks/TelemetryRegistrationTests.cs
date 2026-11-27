using BuildingBlocks.Api.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace UnitTests.BuildingBlocks;

public sealed class TelemetryRegistrationTests
{
    private const string FakeConnectionString =
        "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost/";

    [Fact]
    public void Without_AConnectionString_NothingIsRegistered()
    {
        var services = new ServiceCollection();

        services.AddTelemetry(Configuration(null));

        Assert.Empty(services);
    }

    [Fact]
    public void With_AConnectionString_RequestsAreTraced_ExceptHealth()
    {
        var services = new ServiceCollection();
        services.AddTelemetry(Configuration(FakeConnectionString));

        using var provider = services.BuildServiceProvider();
        var filter = provider.GetRequiredService<IOptionsMonitor<AspNetCoreTraceInstrumentationOptions>>()
            .CurrentValue.Filter;

        Assert.NotNull(filter);
        Assert.False(filter(Request("/health")));
        Assert.True(filter(Request("/api/expenses")));
    }

    [Theory]
    [InlineData("/health", false)]
    [InlineData("/health/ready", false)]
    [InlineData("/healthy", true)]
    [InlineData("/api/system/status", true)]
    [InlineData("/internal/notifications/tick", true)]
    public void Only_TheHealthPing_IsQuiet(string path, bool traced) =>
        Assert.Equal(traced, QuietPaths.ShouldTrace(Request(path)));

    private static IConfiguration Configuration(string? connectionString) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [TelemetryRegistration.ConnectionStringKey] = connectionString })
            .Build();

    private static DefaultHttpContext Request(string path) => new() { Request = { Path = path } };
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace IntegrationTests.TestHost;

/// <summary>
/// Starts the real API on a throwaway PostgreSQL container, with every module migrated. Without
/// Docker the tests are skipped, not failed.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TickKey = "integration-tick-key";
    public const string WebOrigin = "https://web.example";

    private PostgreSqlContainer? _database;

    /// <summary>Why the tests cannot run here, or null when they can.</summary>
    public string? Unavailable { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            _database = new PostgreSqlBuilder("postgres:17-alpine").Build();
            await _database.StartAsync();
            _ = Services; // Starts the host: migrations and sample data run now.
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            Unavailable = $"PostgreSQL container is not available: {exception.GetBaseException().Message}";
        }
    }

    public HttpClient CreateHttpsClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _database!.GetConnectionString());
        builder.UseSetting("Database:InitializeOnStartup", "true");
        builder.UseSetting("OutboxTick:Key", TickKey);
        builder.UseSetting("Cors:Origins:0", WebOrigin);
        // The shared host runs many requests from one address.
        builder.UseSetting("RateLimiting:PublicWrite:PermitLimit", "1000");
        builder.UseSetting("RateLimiting:SignIn:PermitLimit", "1000");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        if (_database is not null)
            await _database.DisposeAsync();
    }
}

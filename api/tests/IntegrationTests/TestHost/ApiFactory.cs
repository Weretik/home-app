using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Application.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace IntegrationTests.TestHost;

/// <summary>
/// Starts the real API on a throwaway PostgreSQL container, with every module migrated. Without
/// Docker the tests are skipped, not failed. Google is replaced by <see cref="FakeGoogleVerifier"/>; the clients it
/// hands out are signed in as <see cref="Vitaliy"/> through the real sign-in endpoint, unless a test asks for an
/// anonymous one (ADR 0037).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TickKey = "integration-tick-key";
    public const string ImportKey = "integration-import-key";
    public const string WebOrigin = "https://web.example";
    public const string GoogleClientId = "integration-client-id";
    public const string Vitaliy = "vitaliy@family.test";
    public const string Masha = "masha@family.test";

    private readonly SemaphoreSlim _signInLock = new(1, 1);
    private string? _token;

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

    /// <summary>A client signed in as <see cref="Vitaliy"/>; the session is shared by every test.</summary>
    public HttpClient CreateHttpsClient() => SignedIn(CreateAnonymousClient());

    /// <summary>A client without a session.</summary>
    public HttpClient CreateAnonymousClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    /// <summary>Adds the shared session of <see cref="Vitaliy"/> to a client, for example one of a derived host.</summary>
    public HttpClient SignedIn(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SharedToken());
        return client;
    }

    /// <summary>Signs <paramref name="email"/> in through <c>POST /api/identity/sign-in</c> and returns the session token.</summary>
    public async Task<string> SignInAsync(string email)
    {
        using var client = CreateAnonymousClient();
        var response = await client.PostAsJsonAsync("/api/identity/sign-in", new { credential = FakeGoogleVerifier.Credential(email) });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    private string SharedToken()
    {
        if (_token is not null)
            return _token;
        _signInLock.Wait();
        try
        {
            return _token ??= SignInAsync(Vitaliy).GetAwaiter().GetResult();
        }
        finally
        {
            _signInLock.Release();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", _database!.GetConnectionString());
        builder.UseSetting("Database:InitializeOnStartup", "true");
        builder.UseSetting("OutboxTick:Key", TickKey);
        builder.UseSetting("Expenses:Import:Key", ImportKey);
        builder.UseSetting("Cors:Origins:0", WebOrigin);
        // The shared host runs many requests from one address.
        builder.UseSetting("RateLimiting:PublicWrite:PermitLimit", "1000");
        builder.UseSetting("RateLimiting:SignIn:PermitLimit", "1000");
        builder.UseSetting("Identity:Google:ClientId", GoogleClientId);
        builder.UseSetting("Identity:AllowedEmails:0", Vitaliy);
        builder.UseSetting("Identity:AllowedEmails:1", Masha);
        builder.ConfigureTestServices(services =>
            services.Replace(ServiceDescriptor.Singleton<IGoogleTokenVerifier, FakeGoogleVerifier>()));
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        _signInLock.Dispose();
        if (_database is not null)
            await _database.DisposeAsync();
    }
}

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HomeApp.Api.Tests;

public sealed class HealthTests(HealthTests.Factory factory) : IClassFixture<HealthTests.Factory>
{
    [Fact]
    public async Task Health_returns_healthy_json()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthBody>();
        Assert.Equal("Healthy", body?.Status);
    }

    [Fact]
    public async Task Health_allows_the_configured_web_origin()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", Factory.WebOrigin);

        var response = await client.SendAsync(request);

        Assert.Equal(Factory.WebOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    private sealed record HealthBody(string Status);

    /// <summary>Runs without a database: an empty connection string turns the database check off.</summary>
    public sealed class Factory : WebApplicationFactory<Program>
    {
        public const string WebOrigin = "https://web.example";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default", "");
            builder.UseSetting("Cors:Origins:0", WebOrigin);
        }
    }
}

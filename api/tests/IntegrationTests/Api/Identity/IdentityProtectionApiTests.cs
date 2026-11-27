using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Identity;

/// <summary>
/// Closed by default: every read of the API needs a session, the scheduler key only reads, and the open endpoints
/// stay open (spec identity/001, R-006, R-009, R-010; ADR 0037).
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class IdentityProtectionApiTests(ApiFactory api)
{
    /// <summary>The documented reads that stay anonymous.</summary>
    private static readonly string[] AnonymousReads = ["/api/system/status"];

    private static readonly string ContractPath = Path.Combine(FindApiRoot(), "docs", "contracts", "openapi.json");

    /// <summary>Every documented read of family data; <c>/api/identity/me</c> is a member's own and is checked apart.</summary>
    public static TheoryData<string> SectionReads() =>
        new(Reads().Where(path => path != "/api/identity/me"));

    public static TheoryData<string> DocumentedReads() => new(Reads());

    private static IEnumerable<string> Reads()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(ContractPath));
        var reads = document.RootElement.GetProperty("paths").EnumerateObject()
            .Where(path => path.Value.TryGetProperty("get", out _))
            .Select(path => path.Name)
            .Where(path => !AnonymousReads.Contains(path))
            .Select(path => path.Replace("{id}", "0195d2c4-0000-7000-8000-000000000001", StringComparison.Ordinal))
            .ToList();
        return reads;
    }

    [SkippableTheory]
    [MemberData(nameof(DocumentedReads))]
    public async Task Every_Read_NeedsASession(string path)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();
        using var forged = api.CreateAnonymousClient();
        forged.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "made-up-token");

        var response = await anonymous.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.Unauthorized, (await forged.GetAsync(path)).StatusCode);
    }

    [SkippableTheory]
    [MemberData(nameof(SectionReads))]
    public async Task The_SchedulerKey_ReadsEverySection(string path)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var deploy = api.CreateAnonymousClient();
        deploy.DefaultRequestHeaders.Add("X-Scheduler-Key", ApiFactory.TickKey);

        var response = await deploy.GetAsync(path);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True((int)response.StatusCode < 500, $"{path}: {(int)response.StatusCode}");
    }

    [SkippableFact]
    public async Task The_SchedulerKey_NeverWrites_AndAWrongKeyReadsNothing()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var deploy = api.CreateAnonymousClient();
        deploy.DefaultRequestHeaders.Add("X-Scheduler-Key", ApiFactory.TickKey);
        using var wrong = api.CreateAnonymousClient();
        wrong.DefaultRequestHeaders.Add("X-Scheduler-Key", "wrong-key");

        var write = await deploy.PostAsJsonAsync("/api/shopping/items", new { text = "Хлеб" });

        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await wrong.GetAsync("/api/expenses")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await deploy.GetAsync("/api/identity/me")).StatusCode);
    }

    [SkippableFact]
    public async Task Health_Status_AndTheContract_StayOpen()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();

        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync("/health")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync("/api/system/status")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await anonymous.GetAsync("/openapi/v1.json")).StatusCode);
    }

    [SkippableFact]
    public async Task A_Write_NeedsASession_Too()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();

        var response = await anonymous.PostAsJsonAsync("/api/shopping/items", new { text = "Хлеб" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string FindApiRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HomeApp.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("HomeApp.slnx not found.");
    }
}

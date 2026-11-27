namespace ArchitectureTests.Http;

/// <summary>
/// Every endpoint needs a signed-in member through the fallback policy of the Identity module (ADR 0037). Only the
/// endpoints below may opt out with <c>AllowAnonymous</c>, each for its reason; a new one fails here until it is
/// listed. <c>IdentityProtectionApiTests</c> checks the same at run time.
/// </summary>
public sealed class AuthorizationRulesTests
{
    /// <summary>Files that call <c>AllowAnonymous</c>, with the reason.</summary>
    private static readonly Dictionary<string, string> AnonymousEndpoints = new()
    {
        ["HealthEndpoints.cs"] = "/health and /api/system/status: deploy and uptime checks, no family data",
        ["ApiDocumentation.cs"] = "/openapi/v1.json: the contract is committed to the public repository anyway",
        ["SignInEndpoint.cs"] = "the way in; rate limit SignIn",
        ["OutboxTickEndpoint.cs"] = "the scheduler key header (ADR 0006)",
        ["NotificationsTickEndpoint.cs"] = "the scheduler key header (ADR 0034)",
        ["TelegramWebhookEndpoint.cs"] = "Telegram's secret header (ADR 0033)",
        ["ImportSheetEndpoint.cs"] = "the import key header, unset in production (ADR 0014)",
    };

    [Fact]
    public void Only_TheListedEndpoints_AreAnonymous()
    {
        var anonymous = SourceFiles.Under("src")
            .Where(file => File.ReadAllText(file).Contains(".AllowAnonymous()", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToHashSet();

        var unlisted = anonymous.Except(AnonymousEndpoints.Keys).ToList();
        var stale = AnonymousEndpoints.Keys.Except(anonymous).ToList();

        Assert.True(unlisted.Count == 0, $"Anonymous without a reason: {string.Join(", ", unlisted)}");
        Assert.True(stale.Count == 0, $"Listed but no longer anonymous: {string.Join(", ", stale)}");
    }

    [Fact]
    public void Nothing_ProtectsOrOpens_AnEndpoint_ExceptTheFallbackPolicy_AndThatList()
    {
        // RequireAuthorization or [Authorize] per endpoint would mean the fallback policy was not trusted, and a
        // policy with another meaning would need its own ADR.
        var violations = SourceFiles.Under("src")
            .Where(file => File.ReadAllText(file) is var text
                && (text.Contains(".RequireAuthorization(", StringComparison.Ordinal) || text.Contains("[Authorize", StringComparison.Ordinal)
                    || text.Contains("[AllowAnonymous", StringComparison.Ordinal)))
            .Select(SourceFiles.Relative);

        Assert.Empty(violations);
    }

    [Fact]
    public void The_IdentityModule_SetsTheFallbackPolicy()
    {
        var module = File.ReadAllText(Path.Combine(Repository.Root, "src/Modules/Identity/Identity.Api/IdentityModule.cs"));

        Assert.Contains("SetFallbackPolicy(", module, StringComparison.Ordinal);
        Assert.Contains(".RequireAuthenticatedUser()", module, StringComparison.Ordinal);
    }
}

namespace BuildingBlocks.Api.RateLimiting;

/// <summary>The <c>RateLimiting</c> configuration section: one fixed window per policy, counted per client address.</summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public FixedWindowLimit PublicWrite { get; set; } = new() { PermitLimit = 10, WindowSeconds = 60 };

    public FixedWindowLimit SignIn { get; set; } = new() { PermitLimit = 5, WindowSeconds = 60 };
}

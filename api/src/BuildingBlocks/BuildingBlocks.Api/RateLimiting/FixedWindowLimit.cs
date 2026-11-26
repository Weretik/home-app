namespace BuildingBlocks.Api.RateLimiting;

/// <summary>At most <see cref="PermitLimit"/> requests per client address in each window of <see cref="WindowSeconds"/>.</summary>
public sealed class FixedWindowLimit
{
    public int PermitLimit { get; set; }

    public int WindowSeconds { get; set; }
}

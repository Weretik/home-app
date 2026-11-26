namespace BuildingBlocks.Api.RateLimiting;

/// <summary>Named rate limits that an endpoint opts into with <c>RequireRateLimiting</c>.</summary>
public static class RateLimitPolicies
{
    /// <summary>Anonymous requests that create something, such as placing an order from the site.</summary>
    public const string PublicWrite = "public-write";

    /// <summary>Password sign-in; slows down guessing on top of the Identity lockout.</summary>
    public const string SignIn = "sign-in";
}

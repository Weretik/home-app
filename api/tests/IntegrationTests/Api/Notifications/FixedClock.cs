namespace IntegrationTests.Api.Notifications;

/// <summary>A clock stopped at a chosen moment, so a tick falls inside a digest window.</summary>
internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

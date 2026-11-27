namespace Notifications.Domain.Schedule;

/// <summary>
/// The one row that remembers when the external scheduler last called the notifications tick. The tick runs every 15
/// minutes, so a gap longer than <see cref="SilentAfter"/> means the scheduler has stopped and no digest will go out;
/// the «Телеграм» page shows that at once (spec notifications/001, R-020).
/// </summary>
public sealed class SchedulerHeartbeat
{
    public const int SingletonId = 1;

    public static readonly TimeSpan SilentAfter = TimeSpan.FromMinutes(60);

    private SchedulerHeartbeat()
    {
    }

    public int Id { get; private set; }

    /// <summary>Null until the first tick.</summary>
    public DateTimeOffset? LastTickAtUtc { get; private set; }

    /// <summary>True when the scheduler never called, or not within <see cref="SilentAfter"/>.</summary>
    public static bool IsSilent(DateTimeOffset? lastTickAtUtc, DateTimeOffset nowUtc) =>
        lastTickAtUtc is null || nowUtc - lastTickAtUtc.Value > SilentAfter;
}

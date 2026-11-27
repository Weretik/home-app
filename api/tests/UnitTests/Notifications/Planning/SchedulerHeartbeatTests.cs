using Notifications.Domain.Schedule;

namespace UnitTests.Notifications.Planning;

// The morning of 27.11 nothing came: GitHub ran the */15 cron once overnight, before the 08:00 window. The status page
// now says so instead of staying quiet.
public sealed class SchedulerHeartbeatTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 27, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_Scheduler_ThatNeverTicked_IsSilent() =>
        Assert.True(SchedulerHeartbeat.IsSilent(null, Now));

    [Fact]
    public void A_Tick_WithinTheLastHour_IsNotSilent() =>
        Assert.False(SchedulerHeartbeat.IsSilent(Now.AddMinutes(-59), Now));

    [Fact]
    public void No_Tick_ForMoreThanAnHour_IsSilent() =>
        Assert.True(SchedulerHeartbeat.IsSilent(Now.AddMinutes(-61), Now));
}

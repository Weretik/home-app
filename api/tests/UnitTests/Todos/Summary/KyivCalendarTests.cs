using Microsoft.Extensions.Time.Testing;
using Todos.Application;

namespace UnitTests.Todos.Summary;

public sealed class KyivCalendarTests
{
    [Fact]
    public void Late_Evening_InUtc_IsAlready_TomorrowInKyiv()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 26, 22, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 11, 27), clock.TodayInKyiv());
    }

    [Fact]
    public void Midday_InUtc_IsTheSameDay_InKyiv()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 7, 1), clock.TodayInKyiv());
    }
}

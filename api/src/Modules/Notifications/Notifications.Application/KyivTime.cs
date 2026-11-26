namespace Notifications.Application;

/// <summary>
/// The family lives in Kyiv: the schedule and "today" follow Kyiv's wall clock, summer time included, whatever time
/// zone the server runs in.
/// </summary>
public static class KyivTime
{
    private static readonly TimeZoneInfo Zone = FindZone();

    public static DateTime Now(TimeProvider clock) => TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Zone).DateTime;

    public static DateOnly Today(TimeProvider clock) => DateOnly.FromDateTime(Now(clock));

    private static TimeZoneInfo FindZone()
    {
        // Older time zone data knows the city only by its former spelling.
        foreach (var id in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }

        return TimeZoneInfo.Utc;
    }
}

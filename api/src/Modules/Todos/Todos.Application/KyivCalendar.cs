namespace Todos.Application;

/// <summary>
/// "Today" for due dates is the calendar day in Kyiv, where the family lives, whatever time zone the server runs in.
/// </summary>
public static class KyivCalendar
{
    private static readonly TimeZoneInfo Zone = FindZone();

    public static DateOnly TodayInKyiv(this TimeProvider clock) => DateOnly.FromDateTime(ToKyiv(clock.GetUtcNow()).DateTime);

    public static DateTimeOffset ToKyiv(DateTimeOffset moment) => TimeZoneInfo.ConvertTime(moment, Zone);

    /// <summary>The UTC moment a calendar day starts in Kyiv (midnight there), for filtering stored UTC times by day.</summary>
    public static DateTimeOffset StartOfDayInKyiv(DateOnly day)
    {
        var midnight = day.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(midnight, Zone.GetUtcOffset(midnight)).ToUniversalTime();
    }

    private static TimeZoneInfo FindZone()
    {
        // Older time zone data knows the city only by its former spelling.
        foreach (var id in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }

        // Neither id is known: let the runtime report the missing zone (a broken host, not a business rule).
        return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
    }
}

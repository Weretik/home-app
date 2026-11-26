namespace Todos.Application;

/// <summary>
/// "Today" for due dates is the calendar day in Kyiv, where the family lives, whatever time zone the server runs in.
/// </summary>
public static class KyivCalendar
{
    private static readonly TimeZoneInfo Zone = FindZone();

    public static DateOnly TodayInKyiv(this TimeProvider clock) => DateOnly.FromDateTime(ToKyiv(clock.GetUtcNow()).DateTime);

    public static DateTimeOffset ToKyiv(DateTimeOffset moment) => TimeZoneInfo.ConvertTime(moment, Zone);

    private static TimeZoneInfo FindZone()
    {
        // Older time zone data knows the city only by its former spelling.
        foreach (var id in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }

        throw new InvalidOperationException("The Europe/Kyiv time zone is not available on this machine.");
    }
}

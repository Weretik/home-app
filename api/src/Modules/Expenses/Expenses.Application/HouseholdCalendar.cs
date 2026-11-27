namespace Expenses.Application;

/// <summary>
/// The household's day: the date in Kyiv, so figures that depend on "today" change at local midnight and a limit change
/// made on the 1st lands in that month. A Kyiv clock is also kept by Gifts, Todos and Notifications; one shared helper
/// is deferred (audit 2026-11-27 AF-007).
/// </summary>
internal static class HouseholdCalendar
{
    // Older time zone data knows the city only by its former spelling; Windows hosts know it by the Windows id.
    private static readonly TimeZoneInfo? Kyiv = FindKyiv();

    public static DateOnly TodayInKyiv(this TimeProvider clock)
    {
        var now = clock.GetUtcNow();
        return Kyiv is null
            ? DateOnly.FromDateTime(now.UtcDateTime)
            : DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Kyiv).DateTime);
    }

    private static TimeZoneInfo? FindKyiv()
    {
        foreach (var id in new[] { "Europe/Kyiv", "Europe/Kiev", "FLE Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }

        // No time zone data at all (a broken host): fall back to UTC, as before, rather than failing every request.
        return null;
    }
}

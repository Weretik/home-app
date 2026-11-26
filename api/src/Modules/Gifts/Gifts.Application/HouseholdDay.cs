namespace Gifts.Application;

/// <summary>"Today" for the household: the date in Kyiv, so it changes at local midnight, not at UTC midnight.</summary>
internal static class HouseholdDay
{
    private const string TimeZoneId = "Europe/Kyiv";

    public static DateOnly Today(TimeProvider clock)
    {
        var now = clock.GetUtcNow();
        try
        {
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId)).DateTime);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(now.UtcDateTime);
        }
    }
}

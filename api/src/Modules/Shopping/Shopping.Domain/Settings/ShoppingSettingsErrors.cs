using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Settings;

public static class ShoppingSettingsErrors
{
    public static readonly Error CartKeepHoursOutOfRange = Error.Validation(
        "shopping.cart_keep_hours_out_of_range",
        $"A bought item stays in the cart {ShoppingSettings.MinCartKeepHours}–{ShoppingSettings.MaxCartKeepHours} hours.",
        "cartKeepHours");

    public static readonly Error FrequentDaysOutOfRange = Error.Validation(
        "shopping.frequent_days_out_of_range",
        $"Frequent items count the last {ShoppingSettings.MinFrequentDays}–{ShoppingSettings.MaxFrequentDays} days.",
        "frequentDays");

    public static readonly Error Missing = Error.NotFound(
        "shopping.settings_not_found", "The shopping settings row is missing; apply the migrations.");
}

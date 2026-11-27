using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Settings;

public static class ShoppingSettingsErrors
{
    public static readonly Error CartKeepHoursOutOfRange = Error.Validation(
        "shopping.cart_keep_hours_out_of_range",
        $"Куплений товар лишається в кошику від {ShoppingSettings.MinCartKeepHours} до {ShoppingSettings.MaxCartKeepHours} годин.",
        "cartKeepHours");

    public static readonly Error FrequentDaysOutOfRange = Error.Validation(
        "shopping.frequent_days_out_of_range",
        $"Часті товари рахуються за період від {ShoppingSettings.MinFrequentDays} до {ShoppingSettings.MaxFrequentDays} днів.",
        "frequentDays");

    public static readonly Error Missing = Error.NotFound(
        "shopping.settings_not_found", "Немає рядка налаштувань покупок; застосуйте міграції.");
}

using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Settings;

/// <summary>The module's settings; exactly one row, with <see cref="SingletonId"/>, created by the first migration.</summary>
public sealed class ShoppingSettings : Entity<ShoppingSettingsId>, IAggregateRoot
{
    public const int DefaultCartKeepHours = 3;
    public const int MinCartKeepHours = 1;
    public const int MaxCartKeepHours = 72;
    public const int DefaultFrequentDays = 183;
    public const int MinFrequentDays = 30;
    public const int MaxFrequentDays = 730;

    public static readonly ShoppingSettingsId SingletonId = new(new Guid("7d1c4f0a-3b2e-4c55-9a1d-6e8f0b2c4a17"));

    private ShoppingSettings()
    {
    }

    /// <summary>How long a bought item stays in the cart before it is history (the bot: 3 hours).</summary>
    public int CartKeepHours { get; private set; }

    /// <summary>How many days back frequent items are counted (the bot: 183, half a year).</summary>
    public int FrequentDays { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static ShoppingSettings Default() =>
        new() { Id = SingletonId, CartKeepHours = DefaultCartKeepHours, FrequentDays = DefaultFrequentDays };

    public Result Update(int cartKeepHours, int frequentDays)
    {
        var check = Result.Combine(
            Result.Ensure(cartKeepHours is >= MinCartKeepHours and <= MaxCartKeepHours, ShoppingSettingsErrors.CartKeepHoursOutOfRange),
            Result.Ensure(frequentDays is >= MinFrequentDays and <= MaxFrequentDays, ShoppingSettingsErrors.FrequentDaysOutOfRange));
        if (check.IsFailure)
            return check;

        CartKeepHours = cartKeepHours;
        FrequentDays = frequentDays;
        return Result.Success();
    }
}

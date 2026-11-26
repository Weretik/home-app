using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Budget;

/// <summary>
/// The household's budget numbers that are not tied to categories: the limit on all expenses of a month, the target
/// for the savings jar and the day limits start carrying overspend. There is exactly one row, with
/// <see cref="SingletonId"/>.
/// </summary>
public sealed class BudgetSettings : Entity<Guid>, IAggregateRoot
{
    public static readonly Guid SingletonId = new("5b0f3a2e-1c4d-4e8a-9f6b-2d7c8e9a0b1c");

    /// <summary>The start of the old «Бюджет» sheet's carry-over (its «Справочник!H2»), used until someone changes it.</summary>
    public static readonly DateOnly SheetLimitsFrom = new(2026, 11, 1);

    private static readonly DateOnly MinLimitsFrom = new(2000, 1, 1);
    private static readonly DateOnly MaxLimitsFrom = new(2100, 12, 31);

    private BudgetSettings()
    {
    }

    /// <summary>The limit on all expenses of a month; 0 means no limit.</summary>
    public decimal MonthlyLimit { get; private set; }

    /// <summary>How much the savings jar should hold; 0 means no goal.</summary>
    public decimal SavingsGoal { get; private set; }

    /// <summary>
    /// The day the limits start counting: overspend carries over from the first month that starts on or after it, and
    /// from the quarter that holds it (see <see cref="LimitCarryOver"/>). Earlier periods carry nothing.
    /// </summary>
    public DateOnly LimitsFrom { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static BudgetSettings Empty() => new() { Id = SingletonId, LimitsFrom = SheetLimitsFrom };

    public Result Update(decimal monthlyLimit, decimal savingsGoal, DateOnly limitsFrom)
    {
        if (!SpendingLimit.IsMoney(monthlyLimit) || !SpendingLimit.IsMoney(savingsGoal))
            return BudgetErrors.SettingsInvalid;
        if (limitsFrom < MinLimitsFrom || limitsFrom > MaxLimitsFrom)
            return BudgetErrors.LimitsFromOutOfRange;

        MonthlyLimit = monthlyLimit;
        SavingsGoal = savingsGoal;
        LimitsFrom = limitsFrom;
        return Result.Success();
    }
}

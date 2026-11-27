using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget.Amounts;

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

    /// <summary>
    /// The limit on all expenses of a month by the month it starts in; an amount of 0 means no limit in that month.
    /// Read one with <see cref="MonthlyLimitFor"/>.
    /// </summary>
    public AmountHistory MonthlyLimits { get; private set; } = AmountHistory.Start(0);

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

    /// <summary>The limit on all expenses of the month that starts on <paramref name="month"/>; 0 means none.</summary>
    public decimal MonthlyLimitFor(DateOnly month) => MonthlyLimits.For(month);

    /// <summary>
    /// Sets the savings goal and the day limits start counting, and <paramref name="monthlyLimit"/> from the month that
    /// holds <paramref name="monthlyLimitFrom"/> on (earlier months keep theirs). An explicit month always replaces the
    /// steps from it on (R-LA-002); without one the change applies from the month of <paramref name="today"/>, and an
    /// amount already in force there changes nothing (R-LA-005).
    /// </summary>
    public Result Update(decimal monthlyLimit, decimal savingsGoal, DateOnly limitsFrom, DateOnly? monthlyLimitFrom, DateOnly today)
    {
        if (!SpendingLimit.IsMoney(monthlyLimit) || !SpendingLimit.IsMoney(savingsGoal))
            return BudgetErrors.SettingsInvalid;
        if (limitsFrom < MinLimitsFrom || limitsFrom > MaxLimitsFrom)
            return BudgetErrors.LimitsFromOutOfRange;
        var day = monthlyLimitFrom ?? today;
        if (!AmountHistory.Covers(day))
            return BudgetErrors.MonthlyLimitFromOutOfRange;

        var month = new DateOnly(day.Year, day.Month, 1);
        if (monthlyLimitFrom is not null || MonthlyLimits.For(month) != monthlyLimit)
            MonthlyLimits = MonthlyLimits.ChangeFrom(month, monthlyLimit);
        SavingsGoal = savingsGoal;
        LimitsFrom = limitsFrom;
        return Result.Success();
    }
}

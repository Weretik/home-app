using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Budget;

/// <summary>
/// The household's budget numbers that are not tied to categories: the limit on all expenses of a month and the
/// target for the savings jar. There is exactly one row, with <see cref="SingletonId"/>.
/// </summary>
public sealed class BudgetSettings : Entity<Guid>, IAggregateRoot
{
    public static readonly Guid SingletonId = new("5b0f3a2e-1c4d-4e8a-9f6b-2d7c8e9a0b1c");

    private BudgetSettings()
    {
    }

    /// <summary>The limit on all expenses of a month; 0 means no limit.</summary>
    public decimal MonthlyLimit { get; private set; }

    /// <summary>How much the savings jar should hold; 0 means no goal.</summary>
    public decimal SavingsGoal { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static BudgetSettings Empty() => new() { Id = SingletonId };

    public Result Update(decimal monthlyLimit, decimal savingsGoal)
    {
        if (!SpendingLimit.IsMoney(monthlyLimit) || !SpendingLimit.IsMoney(savingsGoal))
            return BudgetErrors.SettingsInvalid;

        MonthlyLimit = monthlyLimit;
        SavingsGoal = savingsGoal;
        return Result.Success();
    }
}

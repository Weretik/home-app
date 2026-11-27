using Expenses.Domain.Budget;

namespace Expenses.Domain.Budget.Identifiers;

/// <summary>
/// The id of a <see cref="BudgetSettings"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct BudgetSettingsId(Guid Value) : IComparable<BudgetSettingsId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static BudgetSettingsId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(BudgetSettingsId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

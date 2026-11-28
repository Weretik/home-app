using Expenses.Domain.Entries;

namespace Expenses.Domain.Entries.Identifiers;

/// <summary>
/// The id of an <see cref="Expense"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct ExpenseId(Guid Value) : IComparable<ExpenseId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static ExpenseId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(ExpenseId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

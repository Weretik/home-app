using Expenses.Domain.Entries;

namespace Expenses.Domain.Entries.Identifiers;

/// <summary>
/// The id of an <see cref="Income"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct IncomeId(Guid Value) : IComparable<IncomeId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static IncomeId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(IncomeId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

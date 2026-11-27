namespace Expenses.Domain.Savings;

/// <summary>
/// The id of a <see cref="SavingsMovement"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct SavingsMovementId(Guid Value) : IComparable<SavingsMovementId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static SavingsMovementId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(SavingsMovementId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

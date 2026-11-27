namespace Gifts.Domain.People;

/// <summary>
/// The id of a <see cref="Detail"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct DetailId(Guid Value) : IComparable<DetailId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static DetailId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(DetailId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

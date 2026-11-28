using Gifts.Domain.Wishlist;

namespace Gifts.Domain.Wishlist.Identifiers;

/// <summary>
/// The id of a <see cref="Gift"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct GiftId(Guid Value) : IComparable<GiftId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static GiftId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(GiftId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

namespace Shopping.Domain.Settings;

/// <summary>
/// The id of a <see cref="ShoppingSettings"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct ShoppingSettingsId(Guid Value) : IComparable<ShoppingSettingsId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static ShoppingSettingsId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(ShoppingSettingsId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

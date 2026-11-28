namespace Notifications.Domain.Push;

/// <summary>
/// The id of a <see cref="PushSubscription"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct PushSubscriptionId(Guid Value) : IComparable<PushSubscriptionId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static PushSubscriptionId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(PushSubscriptionId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

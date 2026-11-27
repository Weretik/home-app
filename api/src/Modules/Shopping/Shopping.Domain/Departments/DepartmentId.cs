namespace Shopping.Domain.Departments;

/// <summary>
/// The id of a <see cref="Department"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct DepartmentId(Guid Value) : IComparable<DepartmentId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static DepartmentId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(DepartmentId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

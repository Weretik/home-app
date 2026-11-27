namespace Todos.Domain.Categories;

/// <summary>
/// The id of a <see cref="TodoCategory"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct TodoCategoryId(Guid Value) : IComparable<TodoCategoryId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static TodoCategoryId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(TodoCategoryId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

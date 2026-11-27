using Todos.Domain.Tasks;

namespace Todos.Domain.Tasks.Identifiers;

/// <summary>
/// The id of a <see cref="TodoTask"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct TodoTaskId(Guid Value) : IComparable<TodoTaskId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static TodoTaskId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(TodoTaskId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

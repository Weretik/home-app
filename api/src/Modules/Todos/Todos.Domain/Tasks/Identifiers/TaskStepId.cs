using Todos.Domain.Tasks;

namespace Todos.Domain.Tasks.Identifiers;

/// <summary>
/// The id of a <see cref="TaskStep"/>. A UUID v7 made from the creation time, so ids sort by creation; stored as the plain
/// <c>uuid</c> column by <c>TypedIdConventions</c>.
/// </summary>
public readonly record struct TaskStepId(Guid Value) : IComparable<TaskStepId>
{
    /// <summary>A new id for a record created at <paramref name="now"/>.</summary>
    public static TaskStepId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));

    public int CompareTo(TaskStepId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

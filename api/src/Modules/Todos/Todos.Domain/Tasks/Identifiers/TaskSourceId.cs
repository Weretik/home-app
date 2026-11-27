namespace Todos.Domain.Tasks.Identifiers;

/// <summary>
/// The id of the record a task was made from, in the module named by the task's <c>SourceKind</c> (a gift today).
/// The task only keeps it to find the task again and to report the task done; it never reads that record.
/// </summary>
public readonly record struct TaskSourceId(Guid Value) : IComparable<TaskSourceId>
{
    public int CompareTo(TaskSourceId other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();
}

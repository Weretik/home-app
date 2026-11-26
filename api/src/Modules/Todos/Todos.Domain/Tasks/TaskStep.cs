using BuildingBlocks.Domain.Modeling;

namespace Todos.Domain.Tasks;

/// <summary>
/// One line of a task's checklist. It lives inside its <see cref="TodoTask"/> and changes only through the task's
/// methods, so the task's version moves with every step change.
/// </summary>
public sealed class TaskStep : Entity<Guid>
{
    private TaskStep()
    {
    }

    public string Title { get; private set; } = string.Empty;

    public bool IsDone { get; private set; }

    /// <summary>The position in the checklist: steps keep the order they were added in.</summary>
    public int SortOrder { get; private init; }

    internal static TaskStep Create(string title, int sortOrder, DateTimeOffset now) =>
        new() { Id = Guid.CreateVersion7(now), Title = title, SortOrder = sortOrder };

    internal void Change(string title, bool isDone)
    {
        Title = title;
        IsDone = isDone;
    }
}

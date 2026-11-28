using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Todos.Domain.Categories;
using Todos.Domain.Parties;
using Todos.Domain.Tasks.Identifiers;

namespace Todos.Domain.Tasks;

/// <summary>
/// A household to-do: one row of the old «Все дела» sheet. It is given to one of the two people or to both, may
/// carry a checklist of <see cref="TaskStep"/>s, and moves between active and archived statuses at any time.
/// Every change, steps included, sets <see cref="UpdatedAtUtc"/>, so the row is written and its version changes.
/// </summary>
public sealed class TodoTask : Entity<TodoTaskId>, IAggregateRoot
{
    public const int MaxTitleLength = 200;
    public const int MaxNotesLength = 4000;
    public const int MaxSteps = 50;
    public const int MaxImportKeyLength = 64;
    public const int MaxSourceKindLength = 20;

    private static readonly DateOnly MinDueDate = new(2000, 1, 1);
    private static readonly DateOnly MaxDueDate = new(2100, 12, 31);

    private readonly List<TaskStep> _steps = [];

    private TodoTask()
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public Assignee Assignee { get; private set; }

    /// <summary>Who set the task, when known.</summary>
    public Person? Author { get; private set; }

    public TodoStatus Status { get; private set; }

    public TodoPriority Priority { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public TodoCategoryId? CategoryId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>When the task was done or dropped; null while it is active.</summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }

    /// <summary>The sheet's row id when the task came from the import; a unique index makes a repeat skip it.</summary>
    public string? ImportKey { get; private init; }

    /// <summary>
    /// What module record the task was made for, such as <c>gift</c>; null for a task a person added. Set once
    /// (<see cref="LinkSource"/>); a unique index keeps one task per record (spec gifts/002).
    /// </summary>
    public string? SourceKind { get; private set; }

    /// <summary>The id of that record in its module, stored as a plain value (no foreign key across schemas).</summary>
    public TaskSourceId? SourceId { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    /// <summary>The checklist, in the order the steps were added.</summary>
    public IReadOnlyList<TaskStep> Steps => _steps.OrderBy(s => s.SortOrder).ToList();

    public bool IsArchived => IsArchivedStatus(Status);

    public static bool IsArchivedStatus(TodoStatus status) => status is TodoStatus.Done or TodoStatus.Cancelled;

    public static Result<TodoTask> Create(
        string title,
        string? notes,
        Assignee assignee,
        Person? author,
        TodoStatus status,
        TodoPriority priority,
        DateOnly? dueDate,
        TodoCategoryId? categoryId,
        IReadOnlyCollection<string> steps,
        DateTimeOffset now,
        string? importKey = null)
    {
        var check = Result.Combine(
            Check(title, notes, dueDate),
            Result.Ensure(steps.Count <= MaxSteps, TodoErrors.TooManySteps),
            Result.Ensure(steps.All(IsValidTitle), TodoErrors.StepTitleInvalid),
            Result.Ensure(importKey is not { Length: > MaxImportKeyLength }, TodoErrors.ImportKeyTooLong));
        if (check.IsFailure)
            return check.Error!;

        var task = new TodoTask
        {
            Id = TodoTaskId.New(now),
            Title = title.Trim(),
            Notes = NormalizeNotes(notes),
            Assignee = assignee,
            Author = author,
            Status = TodoStatus.Todo,
            Priority = priority,
            DueDate = dueDate,
            CategoryId = categoryId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ImportKey = importKey,
        };
        task.MoveTo(status, now);
        var order = 0;
        foreach (var step in steps)
            task._steps.Add(TaskStep.Create(step.Trim(), ++order, now));
        return task;
    }

    /// <summary>Replaces what the person can edit, status included; steps, import key and creation time stay.</summary>
    public Result Update(
        string title,
        string? notes,
        Assignee assignee,
        Person? author,
        TodoStatus status,
        TodoPriority priority,
        DateOnly? dueDate,
        TodoCategoryId? categoryId,
        DateTimeOffset now)
    {
        var check = Check(title, notes, dueDate);
        if (check.IsFailure)
            return check;

        Title = title.Trim();
        Notes = NormalizeNotes(notes);
        Assignee = assignee;
        Author = author;
        Priority = priority;
        DueDate = dueDate;
        CategoryId = categoryId;
        MoveTo(status, now);
        UpdatedAtUtc = now;
        return Result.Success();
    }

    /// <summary>
    /// Moves the task to another status. Entering the archive stamps the completion time, leaving it clears the
    /// time, and moving between the two archived statuses keeps the first one.
    /// </summary>
    public void ChangeStatus(TodoStatus status, DateTimeOffset now)
    {
        MoveTo(status, now);
        UpdatedAtUtc = now;
    }

    /// <summary>Records which record of another module the task was made for; only once, on a task without one.</summary>
    public Result LinkSource(string kind, TaskSourceId id)
    {
        if (SourceKind is not null)
            return TodoErrors.SourceAlreadySet;
        if (string.IsNullOrWhiteSpace(kind) || kind.Length > MaxSourceKindLength || id == default)
            return TodoErrors.SourceInvalid;

        SourceKind = kind;
        SourceId = id;
        return Result.Success();
    }

    /// <summary>Adds a step at the end of the checklist.</summary>
    public Result<TaskStep> AddStep(string title, DateTimeOffset now)
    {
        if (!IsValidTitle(title))
            return TodoErrors.StepTitleInvalid;
        if (_steps.Count >= MaxSteps)
            return TodoErrors.TooManySteps;

        var step = TaskStep.Create(title.Trim(), _steps.Count == 0 ? 1 : _steps.Max(s => s.SortOrder) + 1, now);
        _steps.Add(step);
        UpdatedAtUtc = now;
        return step;
    }

    /// <summary>Renames a step and ticks or unticks it.</summary>
    public Result UpdateStep(TaskStepId stepId, string title, bool isDone, DateTimeOffset now)
    {
        var step = _steps.FirstOrDefault(s => s.Id == stepId);
        if (step is null)
            return TodoErrors.StepNotFound(stepId);
        if (!IsValidTitle(title))
            return TodoErrors.StepTitleInvalid;

        step.Change(title.Trim(), isDone);
        UpdatedAtUtc = now;
        return Result.Success();
    }

    public Result RemoveStep(TaskStepId stepId, DateTimeOffset now)
    {
        var step = _steps.FirstOrDefault(s => s.Id == stepId);
        if (step is null)
            return TodoErrors.StepNotFound(stepId);

        _steps.Remove(step);
        UpdatedAtUtc = now;
        return Result.Success();
    }

    /// <summary>Notes are trimmed; empty notes are stored as none.</summary>
    public static string? NormalizeNotes(string? notes) =>
        string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private void MoveTo(TodoStatus status, DateTimeOffset now)
    {
        if (!IsArchivedStatus(status))
            CompletedAtUtc = null;
        else if (!IsArchived || CompletedAtUtc is null)
            CompletedAtUtc = now;
        Status = status;
    }

    private static bool IsValidTitle(string? title) =>
        !string.IsNullOrWhiteSpace(title) && title.Trim().Length <= MaxTitleLength;

    private static Result Check(string title, string? notes, DateOnly? dueDate) =>
        Result.Combine(
            Result.Ensure(IsValidTitle(title), TodoErrors.TitleInvalid),
            Result.Ensure(NormalizeNotes(notes) is not { Length: > MaxNotesLength }, TodoErrors.NotesTooLong),
            Result.Ensure(dueDate is null || (dueDate >= MinDueDate && dueDate <= MaxDueDate), TodoErrors.DueDateOutOfRange));
}

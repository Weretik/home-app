using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Reading;

/// <summary>
/// A task as lists show it, with its category, its steps in order and its version for <c>If-Match</c>. Lists carry
/// notes and steps too, so opening a task needs no second request.
/// </summary>
public sealed record TodoTaskItem(
    Guid Id,
    string Title,
    string? Notes,
    Assignee Assignee,
    Person? Author,
    TodoStatus Status,
    TodoPriority Priority,
    DateOnly? DueDate,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryEmoji,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyList<TaskStepItem> Steps,
    int StepsDone,
    int StepsTotal,
    uint Version);

using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Api.Endpoints.Tasks.Editing;

/// <summary>A new task: its fields, an optional status (Todo when missing) and the titles of its first steps.</summary>
internal sealed record TodoTaskRequest(
    string Title,
    string? Notes,
    Assignee Assignee,
    Person? Author,
    TodoStatus? Status,
    TodoPriority Priority,
    DateOnly? DueDate,
    Guid? CategoryId,
    IReadOnlyList<string>? Steps);

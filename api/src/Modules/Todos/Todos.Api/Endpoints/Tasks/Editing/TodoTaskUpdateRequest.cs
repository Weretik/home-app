using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Api.Endpoints.Tasks.Editing;

/// <summary>The editable fields of a task, status included; steps change through their own endpoints.</summary>
internal sealed record TodoTaskUpdateRequest(
    string Title,
    string? Notes,
    Assignee Assignee,
    Person? Author,
    TodoStatus Status,
    TodoPriority Priority,
    DateOnly? DueDate,
    Guid? CategoryId);

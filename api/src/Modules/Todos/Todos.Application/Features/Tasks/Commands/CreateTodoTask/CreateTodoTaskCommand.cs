using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Application.Idempotency;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.CreateTodoTask;

/// <summary>
/// Adds a task, with its first steps if any. A missing status means <see cref="TodoStatus.Todo"/>. A repeat with the
/// same Idempotency-Key and data returns the first id.
/// </summary>
public sealed record CreateTodoTaskCommand(
    string IdempotencyKey,
    string Title,
    string? Notes,
    Assignee Assignee,
    Person? Author,
    TodoStatus? Status,
    TodoPriority Priority,
    DateOnly? DueDate,
    Guid? CategoryId,
    IReadOnlyList<string> Steps) : ICommand<Result<TodoCreated>>;

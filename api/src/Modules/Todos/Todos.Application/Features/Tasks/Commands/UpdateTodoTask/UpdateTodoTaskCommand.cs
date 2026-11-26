using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.UpdateTodoTask;

/// <summary>Replaces the editable fields of a task, status included, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateTodoTaskCommand(
    Guid Id,
    uint Version,
    string Title,
    string? Notes,
    Assignee Assignee,
    Person? Author,
    TodoStatus Status,
    TodoPriority Priority,
    DateOnly? DueDate,
    Guid? CategoryId) : ICommand<Result>;

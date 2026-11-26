using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Application.Features.Tasks.Reading;

namespace Todos.Application.Features.Tasks.Queries.GetTodoTask;

/// <summary>One task with its steps, active or archived.</summary>
public sealed record GetTodoTaskQuery(Guid Id) : IQuery<Result<TodoTaskItem>>;

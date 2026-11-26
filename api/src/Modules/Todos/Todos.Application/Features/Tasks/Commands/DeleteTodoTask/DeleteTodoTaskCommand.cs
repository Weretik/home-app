using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Tasks.Commands.DeleteTodoTask;

/// <summary>Deletes a task and its steps for good, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record DeleteTodoTaskCommand(Guid Id, uint Version) : ICommand<Result>;

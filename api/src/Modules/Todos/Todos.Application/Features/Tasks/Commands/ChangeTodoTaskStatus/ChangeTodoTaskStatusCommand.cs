using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.ChangeTodoTaskStatus;

/// <summary>Moves a task to another status (also out of the archive), if nobody changed it since <see cref="Version"/>.</summary>
public sealed record ChangeTodoTaskStatusCommand(Guid Id, uint Version, TodoStatus Status) : ICommand<Result>;

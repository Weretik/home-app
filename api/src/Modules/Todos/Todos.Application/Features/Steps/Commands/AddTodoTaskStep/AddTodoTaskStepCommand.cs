using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Application.Idempotency;

namespace Todos.Application.Features.Steps.Commands.AddTodoTaskStep;

/// <summary>Adds a step at the end of a task's checklist. Needs no version: steps never conflict (R-010).</summary>
public sealed record AddTodoTaskStepCommand(Guid TaskId, string Title) : ICommand<Result<TodoCreated>>;

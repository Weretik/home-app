using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Steps.Commands.DeleteTodoTaskStep;

/// <summary>Removes a step from a task's checklist. Needs no version: steps never conflict (R-010).</summary>
public sealed record DeleteTodoTaskStepCommand(Guid TaskId, Guid StepId) : ICommand<Result>;

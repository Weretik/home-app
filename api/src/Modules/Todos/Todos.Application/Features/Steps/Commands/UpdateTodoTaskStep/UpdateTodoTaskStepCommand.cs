using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Steps.Commands.UpdateTodoTaskStep;

/// <summary>Renames a step and ticks or unticks it. Needs no version: steps never conflict (R-010).</summary>
public sealed record UpdateTodoTaskStepCommand(Guid TaskId, Guid StepId, string Title, bool IsDone) : ICommand<Result>;

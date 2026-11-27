using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Domain.Tasks.Identifiers;

namespace Todos.Application.Features.Steps.Commands.UpdateTodoTaskStep;

public sealed class UpdateTodoTaskStepHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateTodoTaskStepCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoTaskStepCommand c, CancellationToken cancellationToken) =>
        await StepChanges.ApplyAsync(
            db, c.TaskId, task => task.UpdateStep(new TaskStepId(c.StepId), c.Title, c.IsDone, clock.GetUtcNow()), cancellationToken);
}

using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Steps.Commands.UpdateTodoTaskStep;

public sealed class UpdateTodoTaskStepHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateTodoTaskStepCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoTaskStepCommand c, CancellationToken cancellationToken) =>
        await StepChanges.ApplyAsync(
            db, c.TaskId, task => task.UpdateStep(c.StepId, c.Title, c.IsDone, clock.GetUtcNow()), cancellationToken);
}

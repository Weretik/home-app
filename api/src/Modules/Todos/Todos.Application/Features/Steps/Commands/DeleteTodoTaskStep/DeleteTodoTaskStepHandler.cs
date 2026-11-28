using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Domain.Tasks.Identifiers;

namespace Todos.Application.Features.Steps.Commands.DeleteTodoTaskStep;

public sealed class DeleteTodoTaskStepHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<DeleteTodoTaskStepCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTodoTaskStepCommand c, CancellationToken cancellationToken) =>
        await StepChanges.ApplyAsync(
            db, c.TaskId, task => task.RemoveStep(new TaskStepId(c.StepId), clock.GetUtcNow()), cancellationToken);
}

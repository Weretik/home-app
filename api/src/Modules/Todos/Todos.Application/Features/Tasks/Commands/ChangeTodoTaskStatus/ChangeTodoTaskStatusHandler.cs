using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Integrations;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.ChangeTodoTaskStatus;

public sealed class ChangeTodoTaskStatusHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<ChangeTodoTaskStatusCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeTodoTaskStatusCommand c, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (task is null)
            return TodoErrors.TaskNotFound(c.Id);
        if (task.Version != c.Version)
            return TodoErrors.ChangedMeanwhile;

        var before = task.Status;
        task.ChangeStatus(c.Status, clock.GetUtcNow());
        TaskCompletion.PublishIfCompleted(db, task, before);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TodoErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

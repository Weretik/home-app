using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.DeleteTodoTask;

public sealed class DeleteTodoTaskHandler(ITodosDbContext db) : ICommandHandler<DeleteTodoTaskCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTodoTaskCommand c, CancellationToken cancellationToken)
    {
        // One conditional DELETE; the steps go with the task (ON DELETE CASCADE).
        var deleted = await db.Tasks.Where(x => x.Id == c.Id && x.Version == c.Version).ExecuteDeleteAsync(cancellationToken);
        if (deleted > 0)
            return Result.Success();

        return await db.Tasks.AnyAsync(x => x.Id == c.Id, cancellationToken)
            ? TodoErrors.ChangedMeanwhile
            : TodoErrors.TaskNotFound(c.Id);
    }
}

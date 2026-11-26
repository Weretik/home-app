using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.UpdateTodoTask;

public sealed class UpdateTodoTaskHandler(ITodosDbContext db, TimeProvider clock) : ICommandHandler<UpdateTodoTaskCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoTaskCommand c, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (task is null)
            return TodoErrors.TaskNotFound(c.Id);
        if (task.Version != c.Version)
            return TodoErrors.ChangedMeanwhile;

        if (c.CategoryId is { } categoryId && !await db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            return CategoryErrors.NotFound(categoryId);

        var updated = task.Update(
            c.Title, c.Notes, c.Assignee, c.Author, c.Status, c.Priority, c.DueDate, c.CategoryId, clock.GetUtcNow());
        if (updated.IsFailure)
            return updated;

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

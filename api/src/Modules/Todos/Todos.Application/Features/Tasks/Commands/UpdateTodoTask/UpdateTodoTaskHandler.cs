using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Integrations;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;
using Todos.Domain.Tasks.Identifiers;

namespace Todos.Application.Features.Tasks.Commands.UpdateTodoTask;

public sealed class UpdateTodoTaskHandler(ITodosDbContext db, TimeProvider clock) : ICommandHandler<UpdateTodoTaskCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoTaskCommand c, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == new TodoTaskId(c.Id), cancellationToken);
        if (task is null)
            return TodoErrors.TaskNotFound(new TodoTaskId(c.Id));
        if (task.Version != c.Version)
            return TodoErrors.ChangedMeanwhile;

        var categoryId = c.CategoryId is { } value ? new TodoCategoryId(value) : (TodoCategoryId?)null;
        if (categoryId is { } category && !await db.Categories.AnyAsync(x => x.Id == category, cancellationToken))
            return CategoryErrors.NotFound(category);

        var before = task.Status;
        var updated = task.Update(
            c.Title, c.Notes, c.Assignee, c.Author, c.Status, c.Priority, c.DueDate, categoryId, clock.GetUtcNow());
        if (updated.IsFailure)
            return updated;
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

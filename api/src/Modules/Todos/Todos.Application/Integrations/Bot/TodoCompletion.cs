using Microsoft.EntityFrameworkCore;
using Todos.Contracts;
using Todos.Domain.Tasks;

namespace Todos.Application.Integrations.Bot;

/// <summary>
/// The Todos side of <see cref="ITodoCompletion"/>: «✅» in Telegram works like «Сделано» on the site, including the
/// event for a task that came from a gift. It needs no version: «done» is a target value. A lost race on the row
/// version reads the task again (up to three times).
/// </summary>
public sealed class TodoCompletion(ITodosDbContext db, TimeProvider clock) : ITodoCompletion
{
    private const int Attempts = 3;

    public async Task<TodoCompletionOutcome> CompleteAsync(Guid taskId, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
            if (task is null)
                return TodoCompletionOutcome.NotFound;
            if (task.IsArchived)
                return TodoCompletionOutcome.AlreadyClosed;

            var before = task.Status;
            task.ChangeStatus(TodoStatus.Done, clock.GetUtcNow());
            TaskCompletion.PublishIfCompleted(db, task, before);
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return TodoCompletionOutcome.Completed;
            }
            catch (DbUpdateConcurrencyException) when (attempt < Attempts)
            {
                db.ChangeTracker.Clear();
            }
        }
    }
}

using BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Todos.Domain.Tasks;
using Todos.Domain.Tasks.Identifiers;

namespace Todos.Application.Features.Steps;

/// <summary>
/// Applies a step change to its task and saves it. Steps take no version (R-010), yet every step change writes the
/// task row, so two changes of the same task sent together race on its <c>xmin</c>. The loser reads the task again
/// and applies its change once more, so ticking steps quickly from two phones never answers 409. Each attempt is one
/// <c>SaveChanges</c>, and only one of them commits.
/// </summary>
internal static class StepChanges
{
    /// <summary>
    /// A change fails only when another change of the same task committed after its read, and each change commits
    /// once, so up to this many changes of one task sent together all land.
    /// </summary>
    public const int MaxAttempts = 10;

    public static async Task<Result> ApplyAsync(
        ITodosDbContext db, Guid taskId, Func<TodoTask, Result> change, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var task = await db.Tasks.Include(t => t.Steps).FirstOrDefaultAsync(t => t.Id == new TodoTaskId(taskId), cancellationToken);
            if (task is null)
                return TodoErrors.TaskNotFound(new TodoTaskId(taskId));

            var changed = change(task);
            if (changed.IsFailure)
                return changed;

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return changed;
            }
            catch (DbUpdateConcurrencyException) when (attempt < MaxAttempts)
            {
                // Another write of this task landed between the read and the save: start again from fresh rows.
                db.ChangeTracker.Clear();
            }
            catch (DbUpdateConcurrencyException)
            {
                return TodoErrors.ChangedMeanwhile;
            }
        }
    }
}

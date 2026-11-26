using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Reading;

/// <summary>
/// The list orders of R-012. Active: priority (Urgent first), due date (earliest first, none last), creation (oldest
/// first). Archive: completion, newest first. Priority is stored as text, so its rank is computed in the query.
/// </summary>
public static class TaskOrder
{
    public static IOrderedQueryable<TaskWithCategory> Active(this IQueryable<TaskWithCategory> rows) =>
        rows
            .OrderBy(r => r.Task.Priority == TodoPriority.Urgent ? 0
                : r.Task.Priority == TodoPriority.High ? 1
                : r.Task.Priority == TodoPriority.Normal ? 2
                : 3)
            .ThenBy(r => r.Task.DueDate == null)
            .ThenBy(r => r.Task.DueDate)
            .ThenBy(r => r.Task.CreatedAtUtc)
            .ThenBy(r => r.Task.Id);

    public static IOrderedQueryable<TaskWithCategory> Archive(this IQueryable<TaskWithCategory> rows) =>
        rows
            .OrderByDescending(r => r.Task.CompletedAtUtc)
            .ThenByDescending(r => r.Task.Id);
}

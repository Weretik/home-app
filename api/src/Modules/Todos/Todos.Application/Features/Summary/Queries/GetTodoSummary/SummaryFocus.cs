using Todos.Application.Features.Tasks.Reading;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Summary.Queries.GetTodoSummary;

/// <summary>
/// Picks the tasks the home page shows first: overdue ones by due date, then the ones due today, then the other
/// urgent ones; inside each group in list order (R-012). Archived tasks never count.
/// </summary>
public static class SummaryFocus
{
    public const int MaxTasks = 8;

    public static IReadOnlyList<TodoTaskItem> Pick(IEnumerable<TodoTaskItem> tasks, DateOnly today) =>
        tasks
            .Where(t => !TodoTask.IsArchivedStatus(t.Status))
            .Select(t => (Task: t, Group: GroupOf(t, today)))
            .Where(x => x.Group is not null)
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Group == 0 ? x.Task.DueDate : null)
            .ThenBy(x => (int)x.Task.Priority)
            .ThenBy(x => x.Task.DueDate is null)
            .ThenBy(x => x.Task.DueDate)
            .ThenBy(x => x.Task.CreatedAtUtc)
            .ThenBy(x => x.Task.Id)
            .Take(MaxTasks)
            .Select(x => x.Task)
            .ToList();

    /// <summary>0 overdue, 1 due today, 2 urgent with a later date or none; null when the task is not in focus.</summary>
    private static int? GroupOf(TodoTaskItem task, DateOnly today) =>
        task.DueDate < today ? 0
        : task.DueDate == today ? 1
        : task.Priority == TodoPriority.Urgent ? 2
        : null;
}

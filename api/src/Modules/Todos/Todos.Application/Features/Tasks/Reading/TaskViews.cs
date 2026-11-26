using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Reading;

/// <summary>The two views of tasks: active (Ждёт, В работе, Отложено) and the archive (Сделано, Не актуально).</summary>
public static class TaskViews
{
    public const string ActiveView = "active";
    public const string ArchiveView = "archive";

    public static IQueryable<TodoTask> WhereActive(this IQueryable<TodoTask> tasks) =>
        tasks.Where(t => t.Status != TodoStatus.Done && t.Status != TodoStatus.Cancelled);

    public static IQueryable<TodoTask> WhereArchived(this IQueryable<TodoTask> tasks) =>
        tasks.Where(t => t.Status == TodoStatus.Done || t.Status == TodoStatus.Cancelled);
}

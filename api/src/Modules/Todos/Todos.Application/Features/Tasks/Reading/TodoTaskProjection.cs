using System.Linq.Expressions;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Reading;

/// <summary>Turns tasks into <see cref="TodoTaskItem"/>s in SQL: a left join to categories and the steps in order.</summary>
public static class TodoTaskProjection
{
    public static readonly Expression<Func<TaskWithCategory, TodoTaskItem>> ToItem = row => new TodoTaskItem(
        row.Task.Id,
        row.Task.Title,
        row.Task.Notes,
        row.Task.Assignee,
        row.Task.Author,
        row.Task.Status,
        row.Task.Priority,
        row.Task.DueDate,
        row.Task.CategoryId,
        row.Category == null ? null : row.Category.Name,
        row.Category == null ? null : row.Category.Emoji,
        row.Task.CreatedAtUtc,
        row.Task.UpdatedAtUtc,
        row.Task.CompletedAtUtc,
        row.Task.Steps.OrderBy(s => s.SortOrder).Select(s => new TaskStepItem(s.Id, s.Title, s.IsDone)).ToList(),
        row.Task.Steps.Count(s => s.IsDone),
        row.Task.Steps.Count(),
        row.Task.Version);

    public static IQueryable<TaskWithCategory> WithCategory(
        this IQueryable<TodoTask> tasks, IQueryable<TodoCategory> categories) =>
        tasks
            .GroupJoin(categories, t => t.CategoryId, c => (Guid?)c.Id, (t, matches) => new { t, matches })
            .SelectMany(
                x => x.matches.DefaultIfEmpty(),
                (x, c) => new TaskWithCategory { Task = x.t, Category = c });
}

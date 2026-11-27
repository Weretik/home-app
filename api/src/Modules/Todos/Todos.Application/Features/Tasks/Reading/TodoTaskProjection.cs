using System.Linq.Expressions;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Reading;

/// <summary>Turns tasks into <see cref="TodoTaskItem"/>s in SQL: a left join to categories and the steps in order.</summary>
public static class TodoTaskProjection
{
    public static readonly Expression<Func<TaskWithCategory, TodoTaskItem>> ToItem = row => new TodoTaskItem(
        row.Task.Id.Value,
        row.Task.Title,
        row.Task.Notes,
        row.Task.Assignee,
        row.Task.Author,
        row.Task.Status,
        row.Task.Priority,
        row.Task.DueDate,
        row.Task.CategoryId.HasValue ? row.Task.CategoryId.Value.Value : null,
        row.Category == null ? null : row.Category.Name,
        row.Category == null ? null : row.Category.Emoji,
        row.Task.CreatedAtUtc,
        row.Task.UpdatedAtUtc,
        row.Task.CompletedAtUtc,
        row.Task.Steps.OrderBy(s => s.SortOrder).Select(s => new TaskStepItem(s.Id.Value, s.Title, s.IsDone)).ToList(),
        row.Task.Steps.Count(s => s.IsDone),
        row.Task.Steps.Count(),
        row.Task.Version,
        row.Task.SourceKind,
        row.Task.SourceId.HasValue ? row.Task.SourceId.Value.Value : null);

    public static IQueryable<TaskWithCategory> WithCategory(
        this IQueryable<TodoTask> tasks, IQueryable<TodoCategory> categories) =>
        tasks
            .GroupJoin(categories, t => t.CategoryId, c => (TodoCategoryId?)c.Id, (t, matches) => new { t, matches })
            .SelectMany(
                x => x.matches.DefaultIfEmpty(),
                (x, c) => new TaskWithCategory { Task = x.t, Category = c });
}

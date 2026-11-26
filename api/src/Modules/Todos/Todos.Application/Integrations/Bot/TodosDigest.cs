using Microsoft.EntityFrameworkCore;
using Todos.Contracts;
using Todos.Domain.Tasks;

namespace Todos.Application.Integrations.Bot;

/// <summary>The Todos side of <see cref="ITodosDigest"/>: the active tasks, read without tracking.</summary>
public sealed class TodosDigest(ITodosDbContext db) : ITodosDigest
{
    public async Task<IReadOnlyList<OpenTodo>> ListActiveAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Tasks.AsNoTracking()
            .Where(t => t.Status != TodoStatus.Done && t.Status != TodoStatus.Cancelled)
            .OrderBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.Priority == TodoPriority.Urgent ? 0 : 1)
            .ThenBy(t => t.CreatedAtUtc)
            .Select(t => new { t.Id, t.Title, t.Assignee, t.DueDate, t.Priority })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(t => new OpenTodo(t.Id, t.Title, Enum.Parse<TodoAssignee>(t.Assignee.ToString()), t.DueDate, t.Priority == TodoPriority.Urgent))];
    }
}

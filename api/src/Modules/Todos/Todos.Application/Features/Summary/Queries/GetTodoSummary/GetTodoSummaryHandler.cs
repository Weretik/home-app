using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Features.Tasks.Reading;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Summary.Queries.GetTodoSummary;

public sealed class GetTodoSummaryHandler(ITodosDbContext db, TimeProvider clock)
    : IQueryHandler<GetTodoSummaryQuery, Result<TodoSummary>>
{
    private static readonly Assignee[] Order = [Assignee.Maria, Assignee.Vitaliy, Assignee.Shared];

    public async ValueTask<Result<TodoSummary>> Handle(GetTodoSummaryQuery query, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var today = clock.TodayInKyiv();
        var active = db.Tasks.AsNoTracking().WhereActive();

        var counts = await active
            .GroupBy(t => t.Assignee)
            .Select(g => new
            {
                Assignee = g.Key,
                Active = g.Count(),
                Overdue = g.Count(t => t.DueDate < today),
                DueToday = g.Count(t => t.DueDate == today),
            })
            .ToListAsync(cancellationToken);

        var weekAgo = now.AddDays(-7);
        var doneLast7Days = await db.Tasks.AsNoTracking().CountAsync(
            t => t.Status == TodoStatus.Done && t.CompletedAtUtc >= weekAgo, cancellationToken);

        // Only overdue, today's and urgent tasks can be in focus. The database orders them as SummaryFocus does and
        // returns at most MaxTasks rows, so a long backlog of overdue tasks is never loaded whole.
        var candidates = await active
            .Where(t => t.DueDate <= today || t.Priority == TodoPriority.Urgent)
            .OrderBy(t => t.DueDate < today ? 0 : t.DueDate == today ? 1 : 2)
            .ThenBy(t => t.DueDate < today ? t.DueDate : null)
            .ThenBy(t => t.Priority == TodoPriority.Urgent ? 0
                : t.Priority == TodoPriority.High ? 1
                : t.Priority == TodoPriority.Normal ? 2
                : 3)
            .ThenBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.CreatedAtUtc)
            .ThenBy(t => t.Id)
            .Take(SummaryFocus.MaxTasks)
            .WithCategory(db.Categories.AsNoTracking())
            .Select(TodoTaskProjection.ToItem)
            .ToListAsync(cancellationToken);

        var assignees = Order
            .Select(a => counts.FirstOrDefault(c => c.Assignee == a) is { } c
                ? new AssigneeCounts(a, c.Active, c.Overdue, c.DueToday)
                : new AssigneeCounts(a, 0, 0, 0))
            .ToList();

        return new TodoSummary(today, assignees, doneLast7Days, SummaryFocus.Pick(candidates, today));
    }
}

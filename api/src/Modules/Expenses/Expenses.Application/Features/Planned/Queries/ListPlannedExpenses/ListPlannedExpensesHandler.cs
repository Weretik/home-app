using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Planned.Queries.ListPlannedExpenses;

/// <summary>A month holds a handful of plans, so the list is not paged.</summary>
public sealed class ListPlannedExpensesHandler(IExpensesDbContext db)
    : IQueryHandler<ListPlannedExpensesQuery, Result<PlannedExpenseList>>
{
    public async ValueTask<Result<PlannedExpenseList>> Handle(ListPlannedExpensesQuery query, CancellationToken cancellationToken)
    {
        var month = new DateOnly(query.Year, query.Month, 1);
        var items = await db.PlannedExpenses.AsNoTracking()
            .Where(p => p.Month == month)
            .Join(db.Categories, p => p.CategoryId, c => c.Id, (p, c) => new { Plan = p, CategoryName = c.Name })
            .OrderBy(x => x.Plan.Name)
            .ThenBy(x => x.Plan.Id)
            .Select(x => new PlannedExpenseItem(
                x.Plan.Id.Value,
                x.Plan.Version,
                x.Plan.Month.Year,
                x.Plan.Month.Month,
                x.Plan.Name,
                x.Plan.CategoryId.Value,
                x.CategoryName,
                x.Plan.Payer,
                x.Plan.Amount))
            .ToListAsync(cancellationToken);

        return new PlannedExpenseList(items, items.Sum(i => i.Amount));
    }
}

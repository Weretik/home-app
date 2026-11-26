using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

public sealed class GetBudgetHandler(IExpensesDbContext db) : IQueryHandler<GetBudgetQuery, Result<BudgetView>>
{
    public async ValueTask<Result<BudgetView>> Handle(GetBudgetQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.BudgetSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? Domain.Budget.BudgetSettings.Empty();
        var limits = await db.SpendingLimits.AsNoTracking()
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);

        return new BudgetView(
            settings.Version,
            settings.MonthlyLimit,
            settings.SavingsGoal,
            limits.Select(l => new SpendingLimitItem(l.Id, l.Version, l.Name, l.Period, l.Amount, l.CategoryIds)).ToList());
    }
}

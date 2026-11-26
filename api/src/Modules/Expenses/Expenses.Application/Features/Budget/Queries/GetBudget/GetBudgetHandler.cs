using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>
/// The settings row and the limits. A missing settings row answers 404, as the edit does, instead of empty settings
/// that could never be saved.
/// </summary>
public sealed class GetBudgetHandler(IExpensesDbContext db) : IQueryHandler<GetBudgetQuery, Result<BudgetView>>
{
    public async ValueTask<Result<BudgetView>> Handle(GetBudgetQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.BudgetSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
            return BudgetErrors.SettingsNotFound;
        var limits = await db.SpendingLimits.AsNoTracking()
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);

        return new BudgetView(
            settings.Version,
            settings.MonthlyLimit,
            settings.SavingsGoal,
            settings.LimitsFrom,
            limits.Select(l => new SpendingLimitItem(l.Id, l.Version, l.Name, l.Period, l.Amount, l.CategoryIds)).ToList());
    }
}

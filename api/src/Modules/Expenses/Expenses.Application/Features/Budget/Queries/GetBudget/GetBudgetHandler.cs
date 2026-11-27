using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>
/// The settings row and the limits. A missing settings row answers 404, as the edit does, instead of empty settings
/// that could never be saved. The amounts shown are those of the period that holds today in Kyiv.
/// </summary>
public sealed class GetBudgetHandler(IExpensesDbContext db, TimeProvider clock) : IQueryHandler<GetBudgetQuery, Result<BudgetView>>
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

        var today = clock.TodayInKyiv();
        return new BudgetView(
            settings.Version,
            settings.MonthlyLimitFor(LimitCarryOver.PeriodStart(LimitPeriod.Month, today)),
            settings.MonthlyLimits.Steps,
            settings.SavingsGoal,
            settings.LimitsFrom,
            limits.Select(l => new SpendingLimitItem(
                l.Id, l.Version, l.Name, l.Period, l.AmountFor(LimitCarryOver.PeriodStart(l.Period, today)), l.Amounts.Steps, l.CategoryIds))
                .ToList());
    }
}

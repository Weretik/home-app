using BuildingBlocks.Domain.Results;
using Expenses.Application.Features.Summary.Report;
using Expenses.Domain.Budget;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Summary.Queries.GetMonthSummary;

/// <summary>
/// Sums in SQL (grouped), then puts the pieces together in memory with <see cref="SummaryFigures"/>. A quarterly limit counts the whole quarter that
/// holds the month. Limits and the monthly total carry overspend from the periods since
/// <see cref="Domain.Budget.BudgetSettings.LimitsFrom"/> (<see cref="LimitCarryOver"/>): one grouped read gives the
/// limit-counted spending per category and month for that whole stretch, and the carry is iterated in memory.
/// Expenses marked «Не в лимиты» count in the totals only. "Today" is the household's day in Kyiv, so the per-day
/// figures change at local midnight.
/// </summary>
public sealed class GetMonthSummaryHandler(IExpensesDbContext db, TimeProvider clock)
    : IQueryHandler<GetMonthSummaryQuery, Result<MonthSummary>>
{
    private const int TopCategoryCount = 5;

    public async ValueTask<Result<MonthSummary>> Handle(GetMonthSummaryQuery query, CancellationToken cancellationToken)
    {
        var from = new DateOnly(query.Year, query.Month, 1);
        var to = from.AddMonths(1);
        var quarter = LimitCarryOver.PeriodStart(LimitPeriod.Quarter, from);
        var today = Today();

        var incomes = await db.Incomes.AsNoTracking()
            .Where(i => i.Date >= from && i.Date < to)
            .GroupBy(i => i.Person)
            .Select(g => new PersonAmount(g.Key, g.Sum(i => i.Amount)))
            .ToListAsync(cancellationToken);

        var byPayer = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= from && e.Date < to)
            .GroupBy(e => e.Payer)
            .Select(g => new PayerAmount(g.Key, g.Sum(e => e.Amount)))
            .ToListAsync(cancellationToken);

        var outsideLimits = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= from && e.Date < to && e.OutsideLimits)
            .GroupBy(e => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(e => e.Amount) })
            .FirstOrDefaultAsync(cancellationToken);

        var monthByCategory = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= from && e.Date < to)
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Amount, cancellationToken);

        var settings = await db.BudgetSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? Domain.Budget.BudgetSettings.Empty();
        var limits = await db.SpendingLimits.AsNoTracking()
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);

        // One grouped read covers every limit period around the month (the widest is its quarter) and every earlier
        // period the carry-over walks through, from the quarter that holds LimitsFrom.
        var historyFrom = Min(quarter, LimitCarryOver.PeriodStart(LimitPeriod.Quarter, settings.LimitsFrom));
        var history = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= historyFrom && e.Date < quarter.AddMonths(3) && !e.OutsideLimits)
            .GroupBy(e => new { e.CategoryId, e.Date.Year, e.Date.Month })
            .Select(g => new { g.Key.CategoryId, g.Key.Year, g.Key.Month, Amount = g.Sum(e => e.Amount) })
            .ToListAsync(cancellationToken);
        var counted = history
            .Select(x => new CategoryMonthAmount(x.CategoryId, new DateOnly(x.Year, x.Month, 1), x.Amount))
            .ToList();

        var planned = await db.PlannedExpenses.AsNoTracking()
            .Where(p => p.Month >= quarter && p.Month < quarter.AddMonths(3))
            .GroupBy(p => new { p.CategoryId, p.Month })
            .Select(g => new { g.Key.CategoryId, g.Key.Month, Amount = g.Sum(p => p.Amount) })
            .ToListAsync(cancellationToken);
        var plannedByMonth = planned
            .Select(x => new CategoryMonthAmount(x.CategoryId, x.Month, x.Amount))
            .ToList();

        var savingsNet = await db.SavingsMovements.AsNoTracking()
            .Where(m => m.Date >= from && m.Date < to)
            .SumAsync(m => m.Amount, cancellationToken);
        var savingsBalance = await db.SavingsMovements.AsNoTracking()
            .Where(m => m.Date < to)
            .SumAsync(m => m.Amount, cancellationToken);

        var names = await db.Categories.AsNoTracking()
            .Where(c => monthByCategory.Keys.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var top = monthByCategory
            .OrderByDescending(x => x.Value)
            .Take(TopCategoryCount)
            .Select(x => new CategoryAmount(x.Key, names.GetValueOrDefault(x.Key, string.Empty), x.Value))
            .ToList();

        var usages = limits
            .Select(limit => SummaryFigures.Usage(limit, from, settings.LimitsFrom, counted, plannedByMonth, today))
            .ToList();
        var (carriedIn, carriesOut) = SummaryFigures.TotalCarry(settings.MonthlyLimit, settings.LimitsFrom, from, counted);

        var totalIncomes = incomes.Sum(i => i.Amount);
        var totalExpenses = byPayer.Sum(p => p.Amount);
        var forecast = SummaryFigures.Forecast(
            from, totalIncomes, totalExpenses, savingsNet, settings.MonthlyLimit, carriedIn, counted, plannedByMonth);

        return new MonthSummary(
            query.Year,
            query.Month,
            totalIncomes,
            incomes.OrderBy(i => i.Person).ToList(),
            totalExpenses,
            byPayer.OrderBy(p => p.Payer).ToList(),
            outsideLimits?.Count ?? 0,
            outsideLimits?.Amount ?? 0,
            settings.MonthlyLimit,
            carriedIn,
            carriesOut,
            SummaryFigures.DaysLeft(from, to, today),
            totalIncomes - totalExpenses - savingsNet,
            savingsNet,
            savingsBalance,
            settings.SavingsGoal,
            usages,
            top,
            forecast);
    }

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

    private DateOnly Today()
    {
        var now = clock.GetUtcNow();
        try
        {
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv")).DateTime);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(now.UtcDateTime);
        }
    }
}

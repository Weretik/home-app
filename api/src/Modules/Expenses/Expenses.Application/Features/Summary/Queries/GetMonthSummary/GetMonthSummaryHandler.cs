using BuildingBlocks.Domain.Results;
using Expenses.Application.Features.Summary.Report;
using Expenses.Domain.Budget;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Summary.Queries.GetMonthSummary;

/// <summary>
/// Sums in SQL (grouped), then puts the pieces together in memory. A quarterly limit counts the whole quarter that
/// holds the month. "Today" is the household's day in Kyiv, so the per-day figures change at local midnight.
/// </summary>
public sealed class GetMonthSummaryHandler(IExpensesDbContext db, TimeProvider clock)
    : IQueryHandler<GetMonthSummaryQuery, Result<MonthSummary>>
{
    private const int TopCategoryCount = 5;

    public async ValueTask<Result<MonthSummary>> Handle(GetMonthSummaryQuery query, CancellationToken cancellationToken)
    {
        var from = new DateOnly(query.Year, query.Month, 1);
        var to = from.AddMonths(1);
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

        var settings = await db.BudgetSettings.AsNoTracking().FirstOrDefaultAsync(cancellationToken)
            ?? Domain.Budget.BudgetSettings.Empty();
        var limits = await db.SpendingLimits.AsNoTracking()
            .OrderBy(l => l.SortOrder).ThenBy(l => l.Name)
            .ToListAsync(cancellationToken);

        // One grouped read covers the month and every limit period: the widest is the quarter around the month.
        var quarter = new DateOnly(query.Year, ((query.Month - 1) / 3 * 3) + 1, 1);
        var byCategoryAndMonth = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= quarter && e.Date < quarter.AddMonths(3))
            .GroupBy(e => new { e.CategoryId, e.Date.Year, e.Date.Month })
            .Select(g => new { g.Key.CategoryId, g.Key.Year, g.Key.Month, Amount = g.Sum(e => e.Amount) })
            .ToListAsync(cancellationToken);

        var savingsNet = await db.SavingsMovements.AsNoTracking()
            .Where(m => m.Date >= from && m.Date < to)
            .SumAsync(m => m.Amount, cancellationToken);
        var savingsBalance = await db.SavingsMovements.AsNoTracking()
            .Where(m => m.Date < to)
            .SumAsync(m => m.Amount, cancellationToken);

        var monthByCategory = byCategoryAndMonth
            .Where(x => x.Year == query.Year && x.Month == query.Month)
            .ToDictionary(x => x.CategoryId, x => x.Amount);
        var names = await db.Categories.AsNoTracking()
            .Where(c => monthByCategory.Keys.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var top = monthByCategory
            .OrderByDescending(x => x.Value)
            .Take(TopCategoryCount)
            .Select(x => new CategoryAmount(x.Key, names.GetValueOrDefault(x.Key, string.Empty), x.Value))
            .ToList();

        var usages = limits.Select(limit =>
        {
            var (start, end) = limit.PeriodAround(from);
            var spent = byCategoryAndMonth
                .Where(x => limit.CategoryIds.Contains(x.CategoryId))
                .Where(x => new DateOnly(x.Year, x.Month, 1) >= start && new DateOnly(x.Year, x.Month, 1) < end)
                .Sum(x => x.Amount);
            return Usage(limit, start, end, spent, today);
        }).ToList();

        var totalIncomes = incomes.Sum(i => i.Amount);
        var totalExpenses = byPayer.Sum(p => p.Amount);
        return new MonthSummary(
            query.Year,
            query.Month,
            totalIncomes,
            incomes.OrderBy(i => i.Person).ToList(),
            totalExpenses,
            byPayer.OrderBy(p => p.Payer).ToList(),
            settings.MonthlyLimit,
            DaysLeft(from, to, today),
            totalIncomes - totalExpenses - savingsNet,
            savingsNet,
            savingsBalance,
            settings.SavingsGoal,
            usages,
            top);
    }

    private static LimitUsage Usage(SpendingLimit limit, DateOnly start, DateOnly end, decimal spent, DateOnly today)
    {
        var left = limit.Amount - spent;
        var days = DaysLeft(start, end, today);
        decimal? perDay = days is { } d ? Math.Max(0, decimal.Round(left / d, 2)) : null;
        var percent = limit.Amount > 0 ? (int)decimal.Round(spent / limit.Amount * 100, MidpointRounding.AwayFromZero) : 0;
        return new LimitUsage(
            limit.Id, limit.Name, limit.Period, start, end.AddDays(-1), limit.Amount, spent, left, percent, perDay);
    }

    /// <summary>Days from today to the end of the period, today included; null when today is outside it.</summary>
    private static int? DaysLeft(DateOnly start, DateOnly end, DateOnly today) =>
        today >= start && today < end ? end.DayNumber - today.DayNumber : null;

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

using Expenses.Application.Features.Summary.Report;
using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;

namespace Expenses.Application.Features.Summary.Queries.GetMonthSummary;

/// <summary>
/// The arithmetic of the month summary over sums already read from the database: per-limit usage with carry-over,
/// days left, the monthly total's carry and the forecast. Pure, so it is unit-tested with a fixed "today".
/// </summary>
internal static class SummaryFigures
{
    /// <summary>One limit's usage in the period that holds <paramref name="month"/>, against that period's amount.</summary>
    /// <param name="counted">Limit-counted spending per category and month (expenses «Не в лимиты» left out).</param>
    /// <param name="planned">Planned amounts per category and month.</param>
    public static LimitUsage Usage(
        SpendingLimit limit,
        DateOnly month,
        DateOnly limitsFrom,
        IReadOnlyList<CategoryMonthAmount> counted,
        IReadOnlyList<CategoryMonthAmount> planned,
        DateOnly today)
    {
        var (start, end) = limit.PeriodAround(month);
        var months = Monthly(counted, limit.CategoryIds.Contains);
        var spent = SpentIn(months, start, end);
        var amount = limit.AmountFor(start);
        var (carriedIn, carriesOut) = LimitCarryOver.For(
            limit.Period, limit.AmountFor, limitsFrom, start, (s, e) => SpentIn(months, s, e));
        var plannedInPeriod = planned
            .Where(x => limit.CategoryIds.Contains(x.CategoryId) && x.Month >= start && x.Month < end)
            .Sum(x => x.Amount);

        var left = amount - spent - carriedIn;
        var days = DaysLeft(start, end, today);
        decimal? perDay = days is { } d ? Math.Max(0, decimal.Round(left / d, 2)) : null;
        var percent = amount > 0
            ? (int)decimal.Round((spent + carriedIn) / amount * 100, MidpointRounding.AwayFromZero)
            : 0;
        return new LimitUsage(
            limit.Id,
            limit.Name,
            limit.Period,
            start,
            end.AddDays(-1),
            amount,
            spent,
            carriedIn,
            left,
            percent,
            perDay,
            carriesOut,
            plannedInPeriod);
    }

    /// <summary>
    /// The monthly limit on all expenses: what the month takes over and passes on, over every counted expense, each
    /// month against its own monthly limit.
    /// </summary>
    public static (decimal CarriedIn, decimal CarriesOut) TotalCarry(
        AmountHistory monthlyLimits, DateOnly limitsFrom, DateOnly month, IReadOnlyList<CategoryMonthAmount> counted)
    {
        var months = Monthly(counted, _ => true);
        return LimitCarryOver.For(LimitPeriod.Month, monthlyLimits.For, limitsFrom, month, (s, e) => SpentIn(months, s, e));
    }

    /// <summary>
    /// The month with its plans: planned, expected expenses and result, and the room left in the monthly limit after
    /// the counted spending, the plans and the carry (0 when there is no monthly limit).
    /// </summary>
    public static MonthForecast Forecast(
        DateOnly month,
        decimal incomes,
        decimal expenses,
        decimal savingsNet,
        decimal monthlyLimit,
        decimal carriedIn,
        IReadOnlyList<CategoryMonthAmount> counted,
        IReadOnlyList<CategoryMonthAmount> planned)
    {
        var plannedInMonth = planned.Where(x => x.Month == month).Sum(x => x.Amount);
        var limitCounted = counted.Where(x => x.Month == month).Sum(x => x.Amount);
        var expected = expenses + plannedInMonth;
        return new MonthForecast(
            plannedInMonth,
            expected,
            incomes - expected - savingsNet,
            monthlyLimit > 0 ? monthlyLimit - limitCounted - plannedInMonth - carriedIn : 0);
    }

    /// <summary>Days from today to the end of the period, today included; null when today is outside it.</summary>
    public static int? DaysLeft(DateOnly start, DateOnly end, DateOnly today) =>
        today >= start && today < end ? end.DayNumber - today.DayNumber : null;

    /// <summary>The spending of the categories that match, per month.</summary>
    private static Dictionary<DateOnly, decimal> Monthly(IEnumerable<CategoryMonthAmount> rows, Func<Guid, bool> counts) =>
        rows.Where(x => counts(x.CategoryId))
            .GroupBy(x => x.Month)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

    private static decimal SpentIn(Dictionary<DateOnly, decimal> months, DateOnly start, DateOnly end)
    {
        var sum = 0m;
        for (var m = start; m < end; m = m.AddMonths(1))
            sum += months.GetValueOrDefault(m);
        return sum;
    }
}

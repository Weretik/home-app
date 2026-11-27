namespace Expenses.Domain.Budget;

/// <summary>
/// The carry-over of the old «Бюджет» sheet (its «Перенос с прошлого мес.» column): what a period spends over its
/// limit is taken from the next period, and what a period leaves unspent first pays that debt off. Starting with the
/// first counted period, <c>carry(next) = max(0, carry + spent - limit)</c>, so the carry never goes below zero and
/// savings are not carried forward. Monthly limits step by month, quarterly ones by quarter, and every period is measured
/// against its own amount (ADR 0041); a period without a limit (amount 0) takes nothing in and passes nothing on, so
/// the carry starts again from 0 after it.
/// </summary>
public static class LimitCarryOver
{
    /// <summary>
    /// The first period that counts: the first month that starts on or after <paramref name="limitsFrom"/>, or the
    /// quarter that holds it.
    /// </summary>
    public static DateOnly FirstPeriod(LimitPeriod period, DateOnly limitsFrom)
    {
        if (period == LimitPeriod.Quarter)
            return PeriodStart(period, limitsFrom);

        var month = new DateOnly(limitsFrom.Year, limitsFrom.Month, 1);
        return month == limitsFrom ? month : month.AddMonths(1);
    }

    /// <summary>The first day of the month or quarter that holds <paramref name="day"/>.</summary>
    public static DateOnly PeriodStart(LimitPeriod period, DateOnly day) =>
        new(day.Year, period == LimitPeriod.Quarter ? ((day.Month - 1) / 3 * 3) + 1 : day.Month, 1);

    /// <summary>How many months one period lasts.</summary>
    public static int Months(LimitPeriod period) => period == LimitPeriod.Quarter ? 3 : 1;

    /// <summary>
    /// What the period that starts on <paramref name="periodStart"/> takes over from the periods before it, and what
    /// it passes on to the next one. Both are 0 for a period before the first counted one, and when the period has no
    /// limit (an amount of 0).
    /// </summary>
    /// <param name="amountOf">The limit amount of the period that starts on the given day.</param>
    /// <param name="spentIn">What was spent against the limit from the first day up to the second (excluded).</param>
    public static (decimal CarriedIn, decimal CarriesOut) For(
        LimitPeriod period,
        Func<DateOnly, decimal> amountOf,
        DateOnly limitsFrom,
        DateOnly periodStart,
        Func<DateOnly, DateOnly, decimal> spentIn)
    {
        ArgumentNullException.ThrowIfNull(amountOf);
        ArgumentNullException.ThrowIfNull(spentIn);
        var first = FirstPeriod(period, limitsFrom);
        var amount = amountOf(periodStart);
        if (amount <= 0 || periodStart < first)
            return (0, 0);

        var months = Months(period);
        var carry = 0m;
        for (var start = first; start < periodStart; start = start.AddMonths(months))
        {
            var limit = amountOf(start);
            carry = limit <= 0 ? 0 : Math.Max(0, carry + spentIn(start, start.AddMonths(months)) - limit);
        }

        var spent = spentIn(periodStart, periodStart.AddMonths(months));
        return (carry, Math.Max(0, carry + spent - amount));
    }
}

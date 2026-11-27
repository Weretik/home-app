using Expenses.Application.Features.Summary.Queries.GetMonthSummary;
using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;

namespace UnitTests.Expenses.Summary;

/// <summary>
/// The month summary's arithmetic with a fixed "today": quarterly usage, days left and per day, the monthly total's
/// carry and the forecast (SDD 004 SC-MS-002/003, SDD 005 SC-CO-009, SC-FC-007).
/// </summary>
public sealed class SummaryFiguresTests
{
    private static readonly Guid Pets = Guid.NewGuid();
    private static readonly Guid Groceries = Guid.NewGuid();
    private static readonly DateOnly LimitsFrom = new(2026, 11, 1);
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    private static SpendingLimit Limit(LimitPeriod period, decimal amount, Guid category) =>
        SpendingLimit.Create("limit", period, amount, [category], 10, Now).Value;

    private static CategoryMonthAmount Row(Guid category, int year, int month, decimal amount) =>
        new(category, new DateOnly(year, month, 1), amount);

    [Fact]
    public void A_Quarterly_Limit_CountsEveryMonthOfItsQuarter_AndNotTheNext()
    {
        var limit = Limit(LimitPeriod.Quarter, 2_500m, Pets);
        CategoryMonthAmount[] counted = [Row(Pets, 2027, 1, 300m), Row(Pets, 2027, 3, 200m), Row(Pets, 2027, 4, 999m), Row(Groceries, 2027, 2, 50m)];

        var usage = SummaryFigures.Usage(limit, new DateOnly(2027, 2, 1), LimitsFrom, counted, [], new DateOnly(2027, 2, 10));

        Assert.Equal(new DateOnly(2027, 1, 1), usage.From);
        Assert.Equal(new DateOnly(2027, 3, 31), usage.To);
        Assert.Equal(500m, usage.Spent);
        Assert.Equal(2_000m, usage.Left);
        Assert.Equal(20, usage.Percent);
        // From 10 February to 31 March, both included: 19 + 31 days.
        Assert.Equal(40m, usage.PerDay);
    }

    [Fact]
    public void PerDay_IsNull_OutsideThePeriod_AndNeverBelowZero_WhenOverspent()
    {
        var limit = Limit(LimitPeriod.Month, 1_000m, Groceries);
        CategoryMonthAmount[] counted = [Row(Groceries, 2026, 11, 1_200m)];

        var past = SummaryFigures.Usage(limit, new DateOnly(2026, 11, 1), LimitsFrom, counted, [], new DateOnly(2026, 12, 5));
        var current = SummaryFigures.Usage(limit, new DateOnly(2026, 11, 1), LimitsFrom, counted, [], new DateOnly(2026, 11, 26));

        Assert.Null(past.PerDay);
        Assert.Equal(-200m, current.Left);
        Assert.Equal(0m, current.PerDay);
        Assert.Equal(120, current.Percent);
        Assert.Equal(200m, current.CarriesOut);
    }

    [Fact]
    public void PerDay_TakesTheCarryIntoAccount_AndRoundsToKopecks()
    {
        // December after a November overspend of 3 000 on a 10 000 limit: 1 000 left over 11 days from the 21st.
        var limit = Limit(LimitPeriod.Month, 10_000m, Groceries);
        CategoryMonthAmount[] counted = [Row(Groceries, 2026, 11, 13_000m), Row(Groceries, 2026, 12, 6_000m)];

        var usage = SummaryFigures.Usage(limit, new DateOnly(2026, 12, 1), LimitsFrom, counted, [], new DateOnly(2026, 12, 21));

        Assert.Equal(3_000m, usage.CarriedIn);
        Assert.Equal(1_000m, usage.Left);
        Assert.Equal(90.91m, usage.PerDay);
        Assert.Equal(90, usage.Percent);
    }

    [Fact]
    public void A_Limit_SumsItsPlans_OverTheMonthsOfItsPeriod()
    {
        var limit = Limit(LimitPeriod.Quarter, 3_000m, Pets);
        CategoryMonthAmount[] planned = [Row(Pets, 2026, 11, 700m), Row(Pets, 2026, 12, 500m), Row(Pets, 2027, 1, 100m), Row(Groceries, 2026, 11, 50m)];

        var usage = SummaryFigures.Usage(limit, new DateOnly(2026, 11, 1), LimitsFrom, [], planned, new DateOnly(2026, 11, 26));

        Assert.Equal(1_200m, usage.Planned);
        Assert.Equal(3_000m, usage.Left);
    }

    [Fact]
    public void DaysLeft_CountsToday_AndIsNull_OutsideThePeriod()
    {
        var start = new DateOnly(2026, 11, 1);
        var end = new DateOnly(2026, 12, 1);

        Assert.Equal(5, SummaryFigures.DaysLeft(start, end, new DateOnly(2026, 11, 26)));
        Assert.Equal(30, SummaryFigures.DaysLeft(start, end, start));
        Assert.Equal(1, SummaryFigures.DaysLeft(start, end, new DateOnly(2026, 11, 30)));
        Assert.Null(SummaryFigures.DaysLeft(start, end, end));
        Assert.Null(SummaryFigures.DaysLeft(start, end, new DateOnly(2026, 10, 31)));
    }

    [Fact]
    public void The_MonthlyTotal_CarriesEveryCountedExpense_AndNothingWithoutALimit()
    {
        CategoryMonthAmount[] counted = [Row(Groceries, 2026, 11, 50_000m), Row(Pets, 2026, 11, 2_000m)];

        var december = SummaryFigures.TotalCarry(AmountHistory.Start(50_000m), LimitsFrom, new DateOnly(2026, 12, 1), counted);
        var noLimit = SummaryFigures.TotalCarry(AmountHistory.Start(0m), LimitsFrom, new DateOnly(2026, 12, 1), counted);

        Assert.Equal((2_000m, 0m), december);
        Assert.Equal((0m, 0m), noLimit);
    }

    [Fact]
    public void A_Lowered_Limit_KeepsTheOldAmount_ForEarlierMonths_AndTheirCarry()
    {
        // SC-LA-001: 7 000 until November, 5 000 from December (changed on 10 December).
        var limit = Limit(LimitPeriod.Month, 7_000m, Groceries);
        limit.Update("limit", LimitPeriod.Month, 5_000m, [Groceries], 10, new DateOnly(2026, 12, 10), new DateOnly(2026, 12, 10));
        CategoryMonthAmount[] counted = [Row(Groceries, 2026, 11, 8_000m), Row(Groceries, 2026, 12, 4_000m)];

        var november = SummaryFigures.Usage(limit, new DateOnly(2026, 11, 1), LimitsFrom, counted, [], new DateOnly(2026, 12, 10));
        var december = SummaryFigures.Usage(limit, new DateOnly(2026, 12, 1), LimitsFrom, counted, [], new DateOnly(2026, 12, 10));

        Assert.Equal((7_000m, -1_000m, 1_000m), (november.Amount, november.Left, november.CarriesOut));
        Assert.Equal((5_000m, 1_000m, 0m, 0m), (december.Amount, december.CarriedIn, december.Left, december.CarriesOut));
        Assert.Equal(100, december.Percent);
    }

    [Fact]
    public void The_MonthlyTotal_CarriesAgainst_EachMonthsOwnLimit()
    {
        var limits = AmountHistory.Start(7_000m).ChangeFrom(new DateOnly(2026, 12, 1), 5_000m);
        CategoryMonthAmount[] counted = [Row(Groceries, 2026, 11, 8_000m), Row(Pets, 2026, 12, 4_000m)];

        Assert.Equal((1_000m, 0m), SummaryFigures.TotalCarry(limits, LimitsFrom, new DateOnly(2026, 12, 1), counted));
    }

    [Fact]
    public void The_Forecast_AddsThePlans_OfTheMonthOnly()
    {
        var month = new DateOnly(2026, 12, 1);
        CategoryMonthAmount[] counted = [Row(Groceries, 2026, 12, 30_000m)];
        CategoryMonthAmount[] planned = [Row(Groceries, 2026, 12, 3_000m), Row(Pets, 2026, 12, 2_000m), Row(Pets, 2027, 1, 9_000m)];

        var forecast = SummaryFigures.Forecast(month, 60_000m, 31_000m, 4_000m, 50_000m, 2_000m, counted, planned);
        var noLimit = SummaryFigures.Forecast(month, 60_000m, 31_000m, 4_000m, 0m, 0m, counted, planned);

        Assert.Equal(5_000m, forecast.Planned);
        Assert.Equal(36_000m, forecast.ExpectedExpenses);
        Assert.Equal(20_000m, forecast.ExpectedResult);
        Assert.Equal(13_000m, forecast.ExpectedTotalLeft);
        Assert.Equal(0m, noLimit.ExpectedTotalLeft);
    }
}

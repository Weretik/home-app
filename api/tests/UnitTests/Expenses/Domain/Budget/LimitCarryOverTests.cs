using Expenses.Domain.Budget;

namespace UnitTests.Expenses.Domain.Budget;

/// <summary>The carry-over of the old «Бюджет» sheet: carry(next) = max(0, carry + spent - limit), each period with its own limit.</summary>
public sealed class LimitCarryOverTests
{
    private static readonly DateOnly LimitsFrom = new(2026, 11, 1);

    [Fact]
    public void The_FirstPeriod_IsTheStartMonth_OrTheNextOne_AndTheQuarterThatHoldsIt()
    {
        Assert.Equal(new DateOnly(2026, 11, 1), LimitCarryOver.FirstPeriod(LimitPeriod.Month, new DateOnly(2026, 11, 1)));
        Assert.Equal(new DateOnly(2026, 12, 1), LimitCarryOver.FirstPeriod(LimitPeriod.Month, new DateOnly(2026, 11, 15)));
        Assert.Equal(new DateOnly(2026, 10, 1), LimitCarryOver.FirstPeriod(LimitPeriod.Quarter, new DateOnly(2026, 11, 1)));
        Assert.Equal(new DateOnly(2027, 1, 1), LimitCarryOver.FirstPeriod(LimitPeriod.Quarter, new DateOnly(2027, 3, 31)));
    }

    [Fact]
    public void The_FirstPeriod_CarriesNothingIn_AndPassesOnItsOverspend()
    {
        var spent = Monthly(("2026-11", 18_000m));

        var carry = LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2026, 11, 1), spent);

        Assert.Equal((0m, 3_000m), carry);
    }

    [Fact]
    public void Overspend_CarriesOver_AndAnUnderspend_PaysItOff_ButSavingsDoNotCarry()
    {
        // Nov over by 3000; Dec under by 1000 pays 1000 of it; Jan under by 5000 pays the rest; Feb starts clean even
        // though January left 3000 unspent.
        var spent = Monthly(("2026-11", 18_000m), ("2026-12", 14_000m), ("2027-01", 10_000m), ("2027-02", 16_000m));

        Assert.Equal((3_000m, 2_000m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2026, 12, 1), spent));
        Assert.Equal((2_000m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2027, 1, 1), spent));
        Assert.Equal((0m, 1_000m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2027, 2, 1), spent));
        Assert.Equal((1_000m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2027, 3, 1), spent));
    }

    [Fact]
    public void A_Big_Overspend_KeepsCarrying_UntilEmptyMonths_PayItOff()
    {
        var spent = Monthly(("2026-11", 40_000m));

        Assert.Equal((25_000m, 10_000m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2026, 12, 1), spent));
        Assert.Equal((10_000m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2027, 1, 1), spent));
        Assert.Equal((0m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2027, 2, 1), spent));
    }

    [Fact]
    public void Periods_BeforeTheStart_AreIgnored()
    {
        var spent = Monthly(("2026-10", 100_000m), ("2026-11", 1_000m));

        Assert.Equal((0m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2026, 11, 1), spent));
        Assert.Equal((0m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(15_000m), LimitsFrom, new DateOnly(2026, 10, 1), spent));
    }

    [Fact]
    public void A_Quarterly_Limit_StepsByQuarter_FromTheQuarterThatHoldsTheStart()
    {
        // Q4 2026 (October counts: the quarter holds the start day) is over by 500; Q1 2027 is over by 200 more.
        var spent = Monthly(("2026-10", 1_000m), ("2026-12", 2_000m), ("2027-02", 2_200m));

        Assert.Equal((0m, 500m), LimitCarryOver.For(LimitPeriod.Quarter, Flat(2_500m), LimitsFrom, new DateOnly(2026, 10, 1), spent));
        Assert.Equal((500m, 200m), LimitCarryOver.For(LimitPeriod.Quarter, Flat(2_500m), LimitsFrom, new DateOnly(2027, 1, 1), spent));
        Assert.Equal((200m, 0m), LimitCarryOver.For(LimitPeriod.Quarter, Flat(2_500m), LimitsFrom, new DateOnly(2027, 4, 1), spent));
    }

    [Fact]
    public void No_Limit_CarriesNothing()
    {
        var spent = Monthly(("2026-11", 100_000m));

        Assert.Equal((0m, 0m), LimitCarryOver.For(LimitPeriod.Month, Flat(0m), LimitsFrom, new DateOnly(2026, 12, 1), spent));
    }

    [Fact]
    public void Every_Period_IsMeasured_AgainstItsOwnAmount()
    {
        // SC-LA-006: 7 000 in November, 5 000 from December; December takes in 1 000 and passes on 0.
        var spent = Monthly(("2026-11", 8_000m), ("2026-12", 4_000m));
        Func<DateOnly, decimal> amount = start => start < new DateOnly(2026, 12, 1) ? 7_000m : 5_000m;

        Assert.Equal((0m, 1_000m), LimitCarryOver.For(LimitPeriod.Month, amount, LimitsFrom, new DateOnly(2026, 11, 1), spent));
        Assert.Equal((1_000m, 0m), LimitCarryOver.For(LimitPeriod.Month, amount, LimitsFrom, new DateOnly(2026, 12, 1), spent));
    }

    [Fact]
    public void A_Period_WithoutALimit_ResetsTheCarry()
    {
        // SC-LA-007: 50 000 until November, none in December, 50 000 again from January; November's overspend stops in December.
        var spent = Monthly(("2026-11", 80_000m), ("2026-12", 10_000m), ("2027-01", 1_000m));
        Func<DateOnly, decimal> amount = start => start == new DateOnly(2026, 12, 1) ? 0m : 50_000m;

        Assert.Equal((0m, 0m), LimitCarryOver.For(LimitPeriod.Month, amount, LimitsFrom, new DateOnly(2026, 12, 1), spent));
        Assert.Equal((0m, 0m), LimitCarryOver.For(LimitPeriod.Month, amount, LimitsFrom, new DateOnly(2027, 1, 1), spent));
    }

    private static Func<DateOnly, decimal> Flat(decimal amount) => _ => amount;

    /// <summary>What was spent between two days, from amounts per month ("yyyy-MM").</summary>
    private static Func<DateOnly, DateOnly, decimal> Monthly(params (string Month, decimal Amount)[] months)
    {
        var byMonth = months.ToDictionary(m => DateOnly.ParseExact(m.Month + "-01", "yyyy-MM-dd"), m => m.Amount);
        return (start, end) => byMonth.Where(m => m.Key >= start && m.Key < end).Sum(m => m.Value);
    }
}

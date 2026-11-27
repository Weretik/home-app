using Expenses.Domain.Budget.Amounts;

namespace UnitTests.Expenses.Amounts;

/// <summary>The amounts of a limit by the period they start in (SDD 006 R-LA-001, R-LA-002, SC-LA-002, SC-LA-003).</summary>
public sealed class AmountHistoryTests
{
    private static readonly DateOnly November = new(2026, 11, 1);
    private static readonly DateOnly December = new(2026, 12, 1);

    [Fact]
    public void A_Started_History_HasOneStep_FromTheBeginning_ForEveryPeriod()
    {
        var history = AmountHistory.Start(7_000m);

        Assert.Equal([new AmountStep(new DateOnly(2000, 1, 1), 7_000m)], history.Steps);
        Assert.Equal(7_000m, history.For(new DateOnly(1990, 1, 1)));
        Assert.Equal(7_000m, history.For(new DateOnly(2100, 12, 1)));
    }

    [Fact]
    public void A_Period_TakesTheLastStep_ThatStartsOnOrBeforeIt()
    {
        var history = AmountHistory.Start(7_000m).ChangeFrom(December, 5_000m);

        Assert.Equal(7_000m, history.For(November));
        Assert.Equal(5_000m, history.For(December));
        Assert.Equal(5_000m, history.For(new DateOnly(2027, 6, 1)));
    }

    [Fact]
    public void A_Change_ReplacesTheSteps_FromItsPeriodOn()
    {
        // SC-LA-002
        var history = AmountHistory.Start(7_000m).ChangeFrom(December, 5_000m);

        var changed = history.ChangeFrom(November, 6_000m);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(November, 6_000m)], changed.Steps);
        Assert.Equal(6_000m, changed.For(December));
    }

    [Fact]
    public void A_Step_EqualToThePeriodBefore_IsNotStored()
    {
        // SC-LA-003
        var history = AmountHistory.Start(7_000m).ChangeFrom(December, 5_000m);

        var changed = history.ChangeFrom(December, 7_000m);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m)], changed.Steps);
    }

    [Fact]
    public void A_Change_FromTheBeginning_ReplacesEverything()
    {
        var history = AmountHistory.Start(7_000m).ChangeFrom(December, 5_000m);

        var changed = history.ChangeFrom(AmountHistory.Beginning, 1_000m);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 1_000m)], changed.Steps);
    }

    [Fact]
    public void A_Change_LeavesTheOriginal_AsItWas()
    {
        var history = AmountHistory.Start(7_000m);

        _ = history.ChangeFrom(December, 5_000m);

        Assert.Single(history.Steps);
    }

    [Fact]
    public void Stored_Steps_AreOrdered_AndAnEmptyList_IsRefused()
    {
        var history = AmountHistory.From([new AmountStep(December, 5_000m), new AmountStep(AmountHistory.Beginning, 7_000m)]);

        Assert.Equal([AmountHistory.Beginning, December], history.Steps.Select(s => s.From));
        Assert.Throws<ArgumentOutOfRangeException>(() => AmountHistory.From([]));
    }

    [Fact]
    public void Stored_Steps_OnTheSameDay_AreMerged_TheLastOneWins()
    {
        var history = AmountHistory.From(
            [new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 5_000m), new AmountStep(December, 6_000m)]);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 6_000m)], history.Steps);
    }

    [Fact]
    public void The_Steps_CannotBeChanged_ThroughTheList()
    {
        var history = AmountHistory.Start(7_000m);

        Assert.False(history.Steps is AmountStep[]);
        Assert.Throws<NotSupportedException>(() => ((IList<AmountStep>)history.Steps)[0] = new AmountStep(December, 1m));
        Assert.Equal(7_000m, history.For(December));
    }

    [Fact]
    public void Histories_WithTheSameSteps_AreEqual()
    {
        var one = AmountHistory.Start(7_000m).ChangeFrom(December, 5_000m);
        var other = AmountHistory.From([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 5_000m)]);

        Assert.Equal(one, other);
        Assert.Equal(one.GetHashCode(), other.GetHashCode());
        Assert.NotEqual(one, AmountHistory.Start(7_000m));
    }

    [Theory]
    [InlineData("1999-12-31", false)]
    [InlineData("2000-01-01", true)]
    [InlineData("2100-12-31", true)]
    [InlineData("2101-01-01", false)]
    public void A_Step_MayStart_Between2000And2100(string day, bool covered) =>
        Assert.Equal(covered, AmountHistory.Covers(DateOnly.Parse(day, System.Globalization.CultureInfo.InvariantCulture)));
}

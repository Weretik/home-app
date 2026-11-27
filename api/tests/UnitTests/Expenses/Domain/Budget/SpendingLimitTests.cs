using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;
using Expenses.Domain.Categories;

namespace UnitTests.Expenses.Domain.Budget;

public sealed class SpendingLimitTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CategoryId Groceries = new(Guid.NewGuid());
    private static readonly CategoryId Household = new(Guid.NewGuid());
    private static readonly DateOnly November = new(2026, 11, 1);
    private static readonly DateOnly December = new(2026, 12, 1);
    private static readonly DateOnly Today = new(2026, 12, 10);

    [Fact]
    public void A_Valid_Limit_IsCreated_WithATrimmedName()
    {
        var result = SpendingLimit.Create("  Еда и быт  ", LimitPeriod.Month, 15_000m, [Groceries, Household], 10, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Еда и быт", result.Value.Name);
        Assert.Equal(LimitPeriod.Month, result.Value.Period);
        Assert.Equal([new AmountStep(AmountHistory.Beginning, 15_000m)], result.Value.Amounts.Steps);
        Assert.Equal([Groceries, Household], result.Value.CategoryIds);
        Assert.Equal(10, result.Value.SortOrder);
        Assert.NotEqual(Guid.Empty, result.Value.Id.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void A_Limit_NeedsAName(string? name)
    {
        var result = SpendingLimit.Create(name!, LimitPeriod.Month, 1m, [Groceries], 10, Now);

        Assert.Equal("expenses.limit_name_required", result.Error!.Code);
    }

    [Fact]
    public void A_Name_LongerThanTheLimit_IsRejected_ButSurroundingSpaces_DoNotCount()
    {
        var tooLong = SpendingLimit.Create(new string('x', SpendingLimit.MaxNameLength + 1), LimitPeriod.Month, 1m, [Groceries], 10, Now);
        var padded = SpendingLimit.Create($"  {new string('x', SpendingLimit.MaxNameLength)}  ", LimitPeriod.Month, 1m, [Groceries], 10, Now);

        Assert.Equal("expenses.limit_name_too_long", tooLong.Error!.Code);
        Assert.True(padded.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.005)]
    [InlineData(10_000_001)]
    public void An_Amount_MustBe_PositiveHryvniasWithKopecks(decimal amount)
    {
        var result = SpendingLimit.Create("Кафе", LimitPeriod.Month, amount, [Groceries], 10, Now);

        Assert.Equal("expenses.limit_amount_invalid", result.Error!.Code);
    }

    [Fact]
    public void The_Largest_Amount_WithKopecks_IsAllowed()
    {
        Assert.True(SpendingLimit.Create("Кафе", LimitPeriod.Month, 10_000_000m, [Groceries], 10, Now).IsSuccess);
        Assert.True(SpendingLimit.Create("Кафе", LimitPeriod.Month, 0.01m, [Groceries], 10, Now).IsSuccess);
    }

    [Fact]
    public void A_Limit_CoversAtLeastOneCategory_EachOnce()
    {
        var none = SpendingLimit.Create("Кафе", LimitPeriod.Month, 1m, [], 10, Now);
        var twice = SpendingLimit.Create("Кафе", LimitPeriod.Month, 1m, [Groceries, Groceries], 10, Now);
        var empty = SpendingLimit.Create("Кафе", LimitPeriod.Month, 1m, [new CategoryId(Guid.Empty)], 10, Now);
        var missing = SpendingLimit.Create("Кафе", LimitPeriod.Month, 1m, null!, 10, Now);

        Assert.Equal("expenses.limit_categories_required", none.Error!.Code);
        Assert.Equal("expenses.limit_categories_required", twice.Error!.Code);
        Assert.Equal("expenses.limit_categories_required", empty.Error!.Code);
        Assert.Equal("expenses.limit_categories_required", missing.Error!.Code);
    }

    [Fact]
    public void An_Update_ReplacesEverything()
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 100m, [Groceries], 10, Now).Value;

        var result = limit.Update(" Уход ", LimitPeriod.Quarter, 3_000m, [Household], 50, December, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal("Уход", limit.Name);
        Assert.Equal(LimitPeriod.Quarter, limit.Period);
        Assert.Equal([new AmountStep(AmountHistory.Beginning, 3_000m)], limit.Amounts.Steps);
        Assert.Equal([Household], limit.CategoryIds);
        Assert.Equal(50, limit.SortOrder);
    }

    [Fact]
    public void A_Failed_Update_KeepsTheOldValues()
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 100m, [Groceries, Household], 10, Now).Value;

        var badAmount = limit.Update("Другое", LimitPeriod.Quarter, -5m, [Household], 20, December, Today);
        var badCategories = limit.Update("Другое", LimitPeriod.Quarter, 5m, [], 20, December, Today);
        var badName = limit.Update(" ", LimitPeriod.Quarter, 5m, [Household], 20, December, Today);

        Assert.True(badAmount.IsFailure);
        Assert.True(badCategories.IsFailure);
        Assert.True(badName.IsFailure);
        Assert.Equal("Еда", limit.Name);
        Assert.Equal(LimitPeriod.Month, limit.Period);
        Assert.Equal(AmountHistory.Start(100m), limit.Amounts);
        Assert.Equal([Groceries, Household], limit.CategoryIds);
        Assert.Equal(10, limit.SortOrder);
    }

    [Fact]
    public void A_New_Amount_AppliesFromThePeriodOfTheGivenDay_AndEarlierPeriods_KeepTheirs()
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 7_000m, [Groceries], 10, Now).Value;

        var result = limit.Update("Еда", LimitPeriod.Month, 5_000m, [Groceries], 10, new DateOnly(2026, 12, 10), Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(7_000m, limit.AmountFor(November));
        Assert.Equal(5_000m, limit.AmountFor(December));
    }

    [Fact]
    public void A_Quarterly_Amount_StartsOnTheQuarter_OfTheGivenDay()
    {
        // SC-LA-004
        var limit = SpendingLimit.Create("Уход", LimitPeriod.Quarter, 3_000m, [Groceries], 10, Now).Value;

        limit.Update("Уход", LimitPeriod.Quarter, 4_000m, [Groceries], 10, new DateOnly(2027, 2, 14), Today);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 3_000m), new AmountStep(new DateOnly(2027, 1, 1), 4_000m)], limit.Amounts.Steps);
    }

    [Fact]
    public void Changing_MonthToQuarter_StartsTheHistoryAgain()
    {
        // SC-LA-005
        var limit = SpendingLimit.Create("Уход", LimitPeriod.Month, 7_000m, [Groceries], 10, Now).Value;
        limit.Update("Уход", LimitPeriod.Month, 5_000m, [Groceries], 10, December, Today);

        limit.Update("Уход", LimitPeriod.Quarter, 18_000m, [Groceries], 10, December, Today);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 18_000m)], limit.Amounts.Steps);
    }

    [Fact]
    public void A_Rename_WithTheAmountInForce_AndNoDay_LeavesTheSteps_Alone()
    {
        // R-LA-005: a later step stays when the name changes and the amount in force today is sent back without a day.
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 7_000m, [Groceries], 10, Now).Value;
        limit.Update("Еда", LimitPeriod.Month, 5_000m, [Groceries], 10, new DateOnly(2027, 1, 1), Today);
        var steps = limit.Amounts;

        limit.Update("Еда и быт", LimitPeriod.Month, 7_000m, [Groceries, Household], 10, null, Today);

        Assert.Equal("Еда и быт", limit.Name);
        Assert.Same(steps, limit.Amounts);
    }

    [Fact]
    public void An_Explicit_Day_WithTheAmountInForce_CancelsALaterStep()
    {
        // R-LA-002: 7 000 now and 5 000 planned from January; 7 000 from December removes the planned decrease.
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 7_000m, [Groceries], 10, Now).Value;
        limit.Update("Еда", LimitPeriod.Month, 5_000m, [Groceries], 10, new DateOnly(2027, 1, 1), Today);

        limit.Update("Еда", LimitPeriod.Month, 7_000m, [Groceries], 10, December, Today);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m)], limit.Amounts.Steps);
        Assert.Equal(7_000m, limit.AmountFor(new DateOnly(2027, 1, 1)));
    }

    [Fact]
    public void Without_A_Day_A_New_Amount_AppliesFromThePeriodOfToday()
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 7_000m, [Groceries], 10, Now).Value;

        limit.Update("Еда", LimitPeriod.Month, 5_000m, [Groceries], 10, null, Today);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 5_000m)], limit.Amounts.Steps);
    }

    [Theory]
    [InlineData(1999, 12, 1)]
    [InlineData(2101, 1, 1)]
    public void An_AmountFrom_OutsideTheSupportedYears_IsRejected(int year, int month, int day)
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 7_000m, [Groceries], 10, Now).Value;

        var result = limit.Update("Еда", LimitPeriod.Month, 5_000m, [Groceries], 10, new DateOnly(year, month, day), Today);

        Assert.Equal("expenses.amount_from_out_of_range", result.Error!.Code);
        Assert.Equal(AmountHistory.Start(7_000m), limit.Amounts);
    }

    [Fact]
    public void The_CategoryIds_AreACopy_OfTheGivenList()
    {
        List<CategoryId> ids = [Groceries];
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 100m, ids, 10, Now).Value;

        ids.Add(Household);

        Assert.Equal([Groceries], limit.CategoryIds);
    }

    [Theory]
    [InlineData("2026-11-26", "2026-11-01", "2026-12-01")]
    [InlineData("2026-12-31", "2026-12-01", "2027-01-01")]
    [InlineData("2026-01-01", "2026-01-01", "2026-02-01")]
    [InlineData("2028-02-29", "2028-02-01", "2028-03-01")]
    public void A_Monthly_Limit_CoversTheCalendarMonth(string day, string start, string end)
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 100m, [Groceries], 10, Now).Value;

        var period = limit.PeriodAround(DateOnly.Parse(day));

        Assert.Equal((DateOnly.Parse(start), DateOnly.Parse(end)), period);
    }

    [Theory]
    [InlineData("2026-11-26", "2026-10-01", "2027-01-01")]
    [InlineData("2026-10-01", "2026-10-01", "2027-01-01")]
    [InlineData("2026-01-01", "2026-01-01", "2026-04-01")]
    [InlineData("2026-03-31", "2026-01-01", "2026-04-01")]
    [InlineData("2026-04-01", "2026-04-01", "2026-07-01")]
    [InlineData("2026-09-30", "2026-07-01", "2026-10-01")]
    public void A_Quarterly_Limit_CoversTheCalendarQuarter(string day, string start, string end)
    {
        var limit = SpendingLimit.Create("Уход", LimitPeriod.Quarter, 3_000m, [Groceries], 10, Now).Value;

        var period = limit.PeriodAround(DateOnly.Parse(day));

        Assert.Equal((DateOnly.Parse(start), DateOnly.Parse(end)), period);
    }
}

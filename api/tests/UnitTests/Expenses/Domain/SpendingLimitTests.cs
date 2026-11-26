using Expenses.Domain.Budget;

namespace UnitTests.Expenses.Domain;

public sealed class SpendingLimitTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Groceries = Guid.NewGuid();
    private static readonly Guid Household = Guid.NewGuid();

    [Fact]
    public void A_Valid_Limit_IsCreated_WithATrimmedName()
    {
        var result = SpendingLimit.Create("  Еда и быт  ", LimitPeriod.Month, 15_000m, [Groceries, Household], 10, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Еда и быт", result.Value.Name);
        Assert.Equal(LimitPeriod.Month, result.Value.Period);
        Assert.Equal(15_000m, result.Value.Amount);
        Assert.Equal([Groceries, Household], result.Value.CategoryIds);
        Assert.Equal(10, result.Value.SortOrder);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
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
        var empty = SpendingLimit.Create("Кафе", LimitPeriod.Month, 1m, [Guid.Empty], 10, Now);
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

        var result = limit.Update(" Уход ", LimitPeriod.Quarter, 3_000m, [Household], 50);

        Assert.True(result.IsSuccess);
        Assert.Equal("Уход", limit.Name);
        Assert.Equal(LimitPeriod.Quarter, limit.Period);
        Assert.Equal(3_000m, limit.Amount);
        Assert.Equal([Household], limit.CategoryIds);
        Assert.Equal(50, limit.SortOrder);
    }

    [Fact]
    public void A_Failed_Update_KeepsTheOldValues()
    {
        var limit = SpendingLimit.Create("Еда", LimitPeriod.Month, 100m, [Groceries, Household], 10, Now).Value;

        var badAmount = limit.Update("Другое", LimitPeriod.Quarter, -5m, [Household], 20);
        var badCategories = limit.Update("Другое", LimitPeriod.Quarter, 5m, [], 20);
        var badName = limit.Update(" ", LimitPeriod.Quarter, 5m, [Household], 20);

        Assert.True(badAmount.IsFailure);
        Assert.True(badCategories.IsFailure);
        Assert.True(badName.IsFailure);
        Assert.Equal("Еда", limit.Name);
        Assert.Equal(LimitPeriod.Month, limit.Period);
        Assert.Equal(100m, limit.Amount);
        Assert.Equal([Groceries, Household], limit.CategoryIds);
        Assert.Equal(10, limit.SortOrder);
    }

    [Fact]
    public void The_CategoryIds_AreACopy_OfTheGivenList()
    {
        List<Guid> ids = [Groceries];
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

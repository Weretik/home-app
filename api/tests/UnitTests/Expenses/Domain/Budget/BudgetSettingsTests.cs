using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;

namespace UnitTests.Expenses.Domain.Budget;

public sealed class BudgetSettingsTests
{
    private static readonly DateOnly November = new(2026, 11, 1);
    private static readonly DateOnly December = new(2026, 12, 1);
    private static readonly DateOnly Today = new(2026, 12, 10);

    [Fact]
    public void Empty_Settings_AreTheSingleton_WithNoLimitAndNoGoal()
    {
        var settings = BudgetSettings.Empty();

        Assert.Equal(BudgetSettings.SingletonId, settings.Id);
        Assert.Equal([new AmountStep(AmountHistory.Beginning, 0m)], settings.MonthlyLimits.Steps);
        Assert.Equal(0m, settings.SavingsGoal);
        Assert.Equal(new DateOnly(2026, 11, 1), settings.LimitsFrom);
    }

    [Theory]
    [InlineData(50_000, 100_000)]
    [InlineData(0, 0)]
    [InlineData(0.01, 10_000_000)]
    public void Settings_AreAmounts_FromZero(decimal monthlyLimit, decimal savingsGoal)
    {
        var settings = BudgetSettings.Empty();

        var result = settings.Update(monthlyLimit, savingsGoal, new DateOnly(2027, 1, 15), December, Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(monthlyLimit, settings.MonthlyLimitFor(December));
        Assert.Equal(savingsGoal, settings.SavingsGoal);
        Assert.Equal(new DateOnly(2027, 1, 15), settings.LimitsFrom);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(10.005, 0)]
    [InlineData(0, 0.001)]
    [InlineData(10_000_001, 0)]
    [InlineData(0, 10_000_001)]
    public void Invalid_Settings_AreRejected_AndTheOldValues_Stay(decimal monthlyLimit, decimal savingsGoal)
    {
        var settings = BudgetSettings.Empty();
        settings.Update(50_000m, 100_000m, BudgetSettings.SheetLimitsFrom, AmountHistory.Beginning, Today);

        var result = settings.Update(monthlyLimit, savingsGoal, new DateOnly(2030, 1, 1), December, Today);

        Assert.Equal("expenses.budget_settings_invalid", result.Error!.Code);
        Assert.Equal([new AmountStep(AmountHistory.Beginning, 50_000m)], settings.MonthlyLimits.Steps);
        Assert.Equal(100_000m, settings.SavingsGoal);
        Assert.Equal(BudgetSettings.SheetLimitsFrom, settings.LimitsFrom);
    }

    [Theory]
    [InlineData(1999, 12, 31)]
    [InlineData(2101, 1, 1)]
    public void A_LimitsFrom_OutsideTheSupportedYears_IsRejected(int year, int month, int day)
    {
        var settings = BudgetSettings.Empty();

        var result = settings.Update(1m, 1m, new DateOnly(year, month, day), December, Today);

        Assert.Equal("expenses.limits_from_out_of_range", result.Error!.Code);
        Assert.Equal(BudgetSettings.SheetLimitsFrom, settings.LimitsFrom);
        Assert.Equal(0m, settings.MonthlyLimitFor(December));
    }

    [Fact]
    public void The_MonthlyLimit_Changes_FromTheMonthOfTheGivenDay_AndEarlierMonths_KeepTheirs()
    {
        var settings = BudgetSettings.Empty();
        settings.Update(7_000m, 0m, BudgetSettings.SheetLimitsFrom, AmountHistory.Beginning, Today);

        var result = settings.Update(5_000m, 0m, BudgetSettings.SheetLimitsFrom, new DateOnly(2026, 12, 10), Today);

        Assert.True(result.IsSuccess);
        Assert.Equal(7_000m, settings.MonthlyLimitFor(November));
        Assert.Equal(5_000m, settings.MonthlyLimitFor(December));
        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 5_000m)], settings.MonthlyLimits.Steps);
    }

    [Fact]
    public void A_MonthlyLimit_AlreadyInForce_WithoutAMonth_LeavesLaterSteps_Alone()
    {
        // R-LA-005
        var settings = BudgetSettings.Empty();
        settings.Update(7_000m, 0m, BudgetSettings.SheetLimitsFrom, AmountHistory.Beginning, Today);
        settings.Update(5_000m, 0m, BudgetSettings.SheetLimitsFrom, new DateOnly(2027, 1, 1), Today);

        settings.Update(7_000m, 1_000m, BudgetSettings.SheetLimitsFrom, null, Today);

        Assert.Equal(1_000m, settings.SavingsGoal);
        Assert.Equal(2, settings.MonthlyLimits.Steps.Count);
        Assert.Equal(5_000m, settings.MonthlyLimitFor(new DateOnly(2027, 1, 1)));
    }

    [Fact]
    public void A_MonthlyLimit_AlreadyInForce_FromAnExplicitMonth_CancelsLaterSteps()
    {
        // R-LA-002
        var settings = BudgetSettings.Empty();
        settings.Update(7_000m, 0m, BudgetSettings.SheetLimitsFrom, AmountHistory.Beginning, Today);
        settings.Update(5_000m, 0m, BudgetSettings.SheetLimitsFrom, new DateOnly(2027, 1, 1), Today);

        settings.Update(7_000m, 0m, BudgetSettings.SheetLimitsFrom, December, Today);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m)], settings.MonthlyLimits.Steps);
    }

    [Fact]
    public void Without_A_Month_The_MonthlyLimit_Changes_FromTheMonthOfToday()
    {
        var settings = BudgetSettings.Empty();
        settings.Update(7_000m, 0m, BudgetSettings.SheetLimitsFrom, AmountHistory.Beginning, Today);

        settings.Update(5_000m, 0m, BudgetSettings.SheetLimitsFrom, null, Today);

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 5_000m)], settings.MonthlyLimits.Steps);
    }

    [Theory]
    [InlineData(1999, 12, 1)]
    [InlineData(2101, 1, 1)]
    public void A_MonthlyLimitFrom_OutsideTheSupportedYears_IsRejected(int year, int month, int day)
    {
        var settings = BudgetSettings.Empty();

        var result = settings.Update(1m, 1m, BudgetSettings.SheetLimitsFrom, new DateOnly(year, month, day), Today);

        Assert.Equal("expenses.monthly_limit_from_out_of_range", result.Error!.Code);
        Assert.True(result.Error!.Fields!.ContainsKey("monthlyLimitFrom"));
        Assert.Equal(0m, settings.MonthlyLimitFor(December));
    }
}

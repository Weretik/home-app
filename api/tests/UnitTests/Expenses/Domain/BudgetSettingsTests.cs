using Expenses.Domain.Budget;

namespace UnitTests.Expenses.Domain;

public sealed class BudgetSettingsTests
{
    [Fact]
    public void Empty_Settings_AreTheSingleton_WithNoLimitAndNoGoal()
    {
        var settings = BudgetSettings.Empty();

        Assert.Equal(BudgetSettings.SingletonId, settings.Id);
        Assert.Equal(0m, settings.MonthlyLimit);
        Assert.Equal(0m, settings.SavingsGoal);
    }

    [Theory]
    [InlineData(50_000, 100_000)]
    [InlineData(0, 0)]
    [InlineData(0.01, 10_000_000)]
    public void Settings_AreAmounts_FromZero(decimal monthlyLimit, decimal savingsGoal)
    {
        var settings = BudgetSettings.Empty();

        var result = settings.Update(monthlyLimit, savingsGoal);

        Assert.True(result.IsSuccess);
        Assert.Equal(monthlyLimit, settings.MonthlyLimit);
        Assert.Equal(savingsGoal, settings.SavingsGoal);
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
        settings.Update(50_000m, 100_000m);

        var result = settings.Update(monthlyLimit, savingsGoal);

        Assert.Equal("expenses.budget_settings_invalid", result.Error!.Code);
        Assert.Equal(50_000m, settings.MonthlyLimit);
        Assert.Equal(100_000m, settings.SavingsGoal);
    }
}

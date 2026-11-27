using Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;
using Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;
using Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;
using Expenses.Domain.Budget;

namespace UnitTests.Expenses.Input;

public sealed class BudgetInputValidatorTests
{
    private static readonly Guid Groceries = Guid.NewGuid();

    [Fact]
    public void A_Create_NeedsAKey_AName_APositiveAmount_AndCategories()
    {
        var validator = new CreateSpendingLimitValidator();

        Assert.True(validator.Validate(new CreateSpendingLimitCommand("key", "Кафе", LimitPeriod.Month, 3_000m, [Groceries])).IsValid);
        Assert.False(validator.Validate(new CreateSpendingLimitCommand("", "Кафе", LimitPeriod.Month, 3_000m, [Groceries])).IsValid);
        Assert.False(validator.Validate(new CreateSpendingLimitCommand("key", "", LimitPeriod.Month, 3_000m, [Groceries])).IsValid);
        Assert.False(validator.Validate(new CreateSpendingLimitCommand("key", "Кафе", LimitPeriod.Month, 0m, [Groceries])).IsValid);
        Assert.False(validator.Validate(new CreateSpendingLimitCommand("key", "Кафе", LimitPeriod.Month, 3_000m, [])).IsValid);
        Assert.False(validator.Validate(new CreateSpendingLimitCommand("key", "Кафе", (LimitPeriod)7, 3_000m, [Groceries])).IsValid);
    }

    [Fact]
    public void An_Update_NeedsTheVersion()
    {
        var validator = new UpdateSpendingLimitValidator();

        Assert.True(validator.Validate(new UpdateSpendingLimitCommand(Guid.NewGuid(), 5, "Кафе", LimitPeriod.Quarter, 1m, [Groceries])).IsValid);
        Assert.False(validator.Validate(new UpdateSpendingLimitCommand(Guid.NewGuid(), 0, "Кафе", LimitPeriod.Quarter, 1m, [Groceries])).IsValid);
        Assert.False(validator.Validate(new UpdateSpendingLimitCommand(Guid.Empty, 5, "Кафе", LimitPeriod.Quarter, 1m, [Groceries])).IsValid);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("2000-01-01", true)]
    [InlineData("2100-12-31", true)]
    [InlineData("1999-12-01", false)]
    [InlineData("2101-01-01", false)]
    public void The_Month_AnAmount_AppliesFrom_IsBetween2000And2100(string? day, bool valid)
    {
        // SC-LA-008
        var from = day is null ? (DateOnly?)null : DateOnly.Parse(day, System.Globalization.CultureInfo.InvariantCulture);

        var limit = new UpdateSpendingLimitValidator().Validate(
            new UpdateSpendingLimitCommand(Guid.NewGuid(), 5, "Кафе", LimitPeriod.Month, 1m, [Groceries], from));
        var settings = new UpdateBudgetSettingsValidator().Validate(new UpdateBudgetSettingsCommand(5, 1m, 0m, null, from));

        Assert.Equal(valid, limit.IsValid);
        Assert.Equal(valid, settings.IsValid);
        if (!valid)
        {
            Assert.Contains(limit.Errors, e => e.PropertyName == "AmountFrom");
            Assert.Contains(settings.Errors, e => e.PropertyName == "MonthlyLimitFrom");
        }
    }

    [Fact]
    public void Settings_NeedTheVersion_AndAmountsFromZero()
    {
        var validator = new UpdateBudgetSettingsValidator();

        Assert.True(validator.Validate(new UpdateBudgetSettingsCommand(5, 0m, 0m, null)).IsValid);
        Assert.True(validator.Validate(new UpdateBudgetSettingsCommand(5, 0m, 0m, new DateOnly(2026, 11, 1))).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(5, 0m, 0m, new DateOnly(1999, 1, 1))).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(0, 50_000m, 100_000m, null)).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(5, -1m, 0m, null)).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(5, 0m, -1m, null)).IsValid);
    }
}

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

    [Fact]
    public void Settings_NeedTheVersion_AndAmountsFromZero()
    {
        var validator = new UpdateBudgetSettingsValidator();

        Assert.True(validator.Validate(new UpdateBudgetSettingsCommand(5, 0m, 0m)).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(0, 50_000m, 100_000m)).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(5, -1m, 0m)).IsValid);
        Assert.False(validator.Validate(new UpdateBudgetSettingsCommand(5, 0m, -1m)).IsValid);
    }
}

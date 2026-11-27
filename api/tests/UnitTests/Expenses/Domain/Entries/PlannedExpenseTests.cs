using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Planning;

namespace UnitTests.Expenses.Domain.Entries;

public sealed class PlannedExpenseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly December = new(2026, 12, 1);
    private static readonly CategoryId Gifts = new(Guid.NewGuid());

    [Fact]
    public void A_Valid_Plan_IsCreated_WithATrimmedName()
    {
        var result = PlannedExpense.Create(December, "  Подарки на НГ  ", Gifts, Payer.Shared, 3_000m, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Подарки на НГ", result.Value.Name);
        Assert.Equal(December, result.Value.Month);
        Assert.Equal((Gifts, Payer.Shared, 3_000m), (result.Value.CategoryId, result.Value.Payer, result.Value.Amount));
        Assert.Equal(Now, result.Value.CreatedAtUtc);
        Assert.NotEqual(Guid.Empty, result.Value.Id.Value);
    }

    [Theory]
    [InlineData("", 1, 1, "expenses.planned_name_required")]
    [InlineData("   ", 1, 1, "expenses.planned_name_required")]
    [InlineData("Подарки", 0, 1, "expenses.amount_not_positive")]
    [InlineData("Подарки", 10.005, 1, "expenses.amount_too_precise")]
    [InlineData("Подарки", 10_000_001, 1, "expenses.amount_too_large")]
    [InlineData("Подарки", 1, 15, "expenses.planned_month_invalid")]
    public void An_Invalid_Plan_IsRejected(string name, decimal amount, int day, string code)
    {
        var result = PlannedExpense.Create(new DateOnly(2026, 12, day), name, Gifts, Payer.Maria, amount, Now);

        Assert.Equal(code, result.Error!.Code);
    }

    [Fact]
    public void A_Name_LongerThanTheLimit_IsRejected()
    {
        var result = PlannedExpense.Create(December, new string('x', PlannedExpense.MaxNameLength + 1), Gifts, Payer.Maria, 1m, Now);

        Assert.Equal("expenses.planned_name_too_long", result.Error!.Code);
    }

    [Fact]
    public void A_Failed_Update_KeepsTheOldValues()
    {
        var plan = PlannedExpense.Create(December, "Подарки", Gifts, Payer.Shared, 3_000m, Now).Value;
        var other = new CategoryId(Guid.NewGuid());

        Assert.Equal("expenses.planned_name_required", plan.Update(December, " ", other, Payer.Maria, 10m).Error!.Code);
        Assert.Equal(("Подарки", Gifts, Payer.Shared, 3_000m), (plan.Name, plan.CategoryId, plan.Payer, plan.Amount));

        Assert.True(plan.Update(new DateOnly(2027, 1, 1), "Ёлка", other, Payer.Maria, 10m).IsSuccess);
        Assert.Equal((new DateOnly(2027, 1, 1), "Ёлка", other, Payer.Maria, 10m), (plan.Month, plan.Name, plan.CategoryId, plan.Payer, plan.Amount));
    }

    [Fact]
    public void Recording_APlan_GivesAnExpense_InTheLimits_WithThePlansPayerCategoryAndAmount()
    {
        var plan = PlannedExpense.Create(December, "Подарки", Gifts, Payer.Maria, 3_000m, Now).Value;
        var paid = new DateOnly(2026, 12, 20);

        var expense = plan.Record(paid, PaymentMethod.Cash, "  ярмарка ", Now.AddDays(1));

        Assert.True(expense.IsSuccess);
        Assert.Equal((paid, Payer.Maria, Gifts, 3_000m, PaymentMethod.Cash, "ярмарка"),
            (expense.Value.Date, expense.Value.Payer, expense.Value.CategoryId, expense.Value.Amount, expense.Value.PaymentMethod, expense.Value.Comment));
        Assert.False(expense.Value.OutsideLimits);
        Assert.NotEqual(plan.Id.Value, expense.Value.Id.Value);
    }

    [Fact]
    public void Recording_OnADay_OutOfRange_IsRejected()
    {
        var plan = PlannedExpense.Create(December, "Подарки", Gifts, Payer.Maria, 3_000m, Now).Value;

        var expense = plan.Record(new DateOnly(1999, 12, 31), PaymentMethod.Card, null, Now);

        Assert.Equal("expenses.date_out_of_range", expense.Error!.Code);
    }
}

using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace UnitTests.Expenses.Domain.Entries;

public sealed class ExpenseTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly CategoryId Groceries = new(Guid.NewGuid());

    [Fact]
    public void A_Valid_Expense_IsCreated_WithATrimmedComment()
    {
        var result = Expense.Create(new DateOnly(2026, 10, 1), Payer.Shared, Groceries, 442m, PaymentMethod.Card, "  АТБ  ", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("АТБ", result.Value.Comment);
        Assert.Equal(Payer.Shared, result.Value.Payer);
        Assert.Equal(Now, result.Value.CreatedAtUtc);
    }

    [Fact]
    public void An_Empty_Comment_IsStored_AsNoComment()
    {
        var result = Expense.Create(new DateOnly(2026, 10, 1), Payer.Maria, Groceries, 1m, PaymentMethod.Cash, "   ", Now);

        Assert.Null(result.Value.Comment);
    }

    [Theory]
    [InlineData(0, "expenses.amount_not_positive")]
    [InlineData(-5, "expenses.amount_not_positive")]
    [InlineData(10.005, "expenses.amount_too_precise")]
    [InlineData(10_000_001, "expenses.amount_too_large")]
    public void An_Amount_MustBe_PositiveHryvniasWithKopecks(decimal amount, string code)
    {
        var result = Expense.Create(new DateOnly(2026, 10, 1), Payer.Vitaliy, Groceries, amount, PaymentMethod.Card, null, Now);

        Assert.Equal(code, result.Error!.Code);
    }

    [Fact]
    public void A_Comment_LongerThanTheLimit_IsRejected()
    {
        var result = Expense.Create(
            new DateOnly(2026, 10, 1), Payer.Vitaliy, Groceries, 1m, PaymentMethod.Card, new string('x', EntryRules.MaxCommentLength + 1), Now);

        Assert.Equal("expenses.comment_too_long", result.Error!.Code);
    }

    [Fact]
    public void An_Expense_CountsInTheLimits_UnlessMarkedOutsideLimits()
    {
        var day = new DateOnly(2026, 11, 10);

        var usual = Expense.Create(day, Payer.Shared, Groceries, 100m, PaymentMethod.Card, null, Now).Value;
        var oneOff = Expense.Create(day, Payer.Shared, Groceries, 9_000m, PaymentMethod.Card, "холодильник", Now, outsideLimits: true).Value;

        Assert.False(usual.OutsideLimits);
        Assert.True(oneOff.OutsideLimits);
    }

    [Fact]
    public void An_Edit_SetsAndClears_OutsideLimits_ButAFailedEdit_KeepsIt()
    {
        var day = new DateOnly(2026, 11, 10);
        var expense = Expense.Create(day, Payer.Shared, Groceries, 100m, PaymentMethod.Card, null, Now).Value;

        Assert.True(expense.Update(day, Payer.Shared, Groceries, 100m, PaymentMethod.Card, null, outsideLimits: true).IsSuccess);
        Assert.True(expense.OutsideLimits);

        Assert.False(expense.Update(day, Payer.Shared, Groceries, -1m, PaymentMethod.Card, null, outsideLimits: false).IsSuccess);
        Assert.True(expense.OutsideLimits);

        Assert.True(expense.Update(day, Payer.Shared, Groceries, 100m, PaymentMethod.Card, null, outsideLimits: false).IsSuccess);
        Assert.False(expense.OutsideLimits);
    }
}

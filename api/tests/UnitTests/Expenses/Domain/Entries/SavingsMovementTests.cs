using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Savings;

namespace UnitTests.Expenses.Domain.Entries;

public sealed class SavingsMovementTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Day = new(2026, 10, 3);

    [Theory]
    [InlineData(500, SavingsDirection.Deposit)]
    [InlineData(-111.10, SavingsDirection.Withdrawal)]
    public void The_Sign_TellsADeposit_FromAWithdrawal(decimal amount, SavingsDirection direction)
    {
        var result = SavingsMovement.Create(Day, Person.Vitaliy, amount, "NovaPay", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(amount, result.Value.Amount);
        Assert.Equal(direction, result.Value.Direction);
    }

    [Theory]
    [InlineData(0, "expenses.amount_zero")]
    [InlineData(-10.005, "expenses.amount_too_precise")]
    [InlineData(-10_000_001, "expenses.amount_too_large")]
    public void A_Movement_NeedsAnAmount_ThatFitsTheRules(decimal amount, string code)
    {
        var result = SavingsMovement.Create(Day, Person.Vitaliy, amount, null, Now);

        Assert.Equal(code, result.Error!.Code);
    }

    [Fact]
    public void An_Update_ChangesTheFields_AndKeepsTheImportKey()
    {
        var movement = SavingsMovement.Create(Day, Person.Vitaliy, -608m, null, Now, "sheet:savings:abc:1").Value;

        var result = movement.Update(new DateOnly(2026, 10, 4), Person.Maria, 608m, "  вернули  ");

        Assert.True(result.IsSuccess);
        Assert.Equal(Person.Maria, movement.Person);
        Assert.Equal(SavingsDirection.Deposit, movement.Direction);
        Assert.Equal("вернули", movement.Comment);
        Assert.Equal("sheet:savings:abc:1", movement.ImportKey);
    }

    [Fact]
    public void A_Failed_Update_ChangesNothing()
    {
        var movement = SavingsMovement.Create(Day, Person.Vitaliy, -608m, "кафе", Now).Value;

        var result = movement.Update(Day, Person.Vitaliy, 0m, "ноль");

        Assert.Equal("expenses.amount_zero", result.Error!.Code);
        Assert.Equal(-608m, movement.Amount);
        Assert.Equal("кафе", movement.Comment);
    }

    [Fact]
    public void An_Expense_Update_RejectsANonPositiveAmount_AndKeepsTheOldValues()
    {
        var expense = Expense.Create(Day, Payer.Shared, Guid.NewGuid(), 100m, PaymentMethod.Card, "АТБ", Now).Value;
        var category = Guid.NewGuid();

        Assert.Equal("expenses.amount_not_positive",
            expense.Update(Day, Payer.Maria, category, -1m, PaymentMethod.Cash, null, false).Error!.Code);
        Assert.Equal(100m, expense.Amount);

        Assert.True(expense.Update(Day, Payer.Maria, category, 55.5m, PaymentMethod.Cash, " ", false).IsSuccess);
        Assert.Equal((Payer.Maria, category, 55.5m, PaymentMethod.Cash), (expense.Payer, expense.CategoryId, expense.Amount, expense.PaymentMethod));
        Assert.Null(expense.Comment);
    }
}

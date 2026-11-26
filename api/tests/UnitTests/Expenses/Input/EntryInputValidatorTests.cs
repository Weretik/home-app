using Expenses.Application.Features.Expenses.Commands.CreateExpense;
using Expenses.Application.Features.Expenses.Commands.UpdateExpense;
using Expenses.Application.Features.Incomes.Commands.CreateIncome;
using Expenses.Application.Features.Incomes.Commands.UpdateIncome;
using Expenses.Application.Features.Savings.Commands.CreateSavingsMovement;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace UnitTests.Expenses.Input;

/// <summary>The shape checks of the entry writes (SDD 001 GAP-003, SDD 002): key, version, enums, amounts, comment.</summary>
public sealed class EntryInputValidatorTests
{
    private static readonly DateOnly Day = new(2026, 11, 26);
    private static readonly Guid Category = Guid.NewGuid();
    private static readonly string LongComment = new('x', EntryRules.MaxCommentLength + 1);

    [Fact]
    public void An_Expense_Create_NeedsAKey_ACategory_APositiveAmount_AndKnownEnums()
    {
        var validator = new CreateExpenseValidator();
        var valid = new CreateExpenseCommand("key", Day, Payer.Shared, Category, 10m, PaymentMethod.Card, "АТБ", OutsideLimits: true);

        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(validator.Validate(valid with { IdempotencyKey = "" }).IsValid);
        Assert.False(validator.Validate(valid with { CategoryId = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(valid with { Amount = 0m }).IsValid);
        Assert.False(validator.Validate(valid with { Payer = (Payer)9 }).IsValid);
        Assert.False(validator.Validate(valid with { PaymentMethod = (PaymentMethod)9 }).IsValid);
        Assert.False(validator.Validate(valid with { Comment = LongComment }).IsValid);
    }

    [Fact]
    public void An_Expense_Update_NeedsTheIdAndTheVersion()
    {
        var validator = new UpdateExpenseValidator();
        var valid = new UpdateExpenseCommand(Guid.NewGuid(), 7, Day, Payer.Maria, Category, 10m, PaymentMethod.Cash, null, false);

        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(validator.Validate(valid with { Version = 0 }).IsValid);
        Assert.False(validator.Validate(valid with { Id = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(valid with { Amount = -1m }).IsValid);
    }

    [Fact]
    public void An_Income_NeedsAKeyOrAVersion_AndAPositiveAmount()
    {
        var create = new CreateIncomeValidator();
        var update = new UpdateIncomeValidator();
        var created = new CreateIncomeCommand("key", Day, Person.Vitaliy, 25_000m, null);
        var updated = new UpdateIncomeCommand(Guid.NewGuid(), 3, Day, Person.Maria, 1m, null);

        Assert.True(create.Validate(created).IsValid);
        Assert.False(create.Validate(created with { IdempotencyKey = " " }).IsValid);
        Assert.False(create.Validate(created with { Amount = 0m }).IsValid);
        Assert.False(create.Validate(created with { Person = (Person)9 }).IsValid);
        Assert.True(update.Validate(updated).IsValid);
        Assert.False(update.Validate(updated with { Version = 0 }).IsValid);
    }

    [Fact]
    public void A_SavingsMovement_IsSigned_ButNeverZero()
    {
        var validator = new CreateSavingsMovementValidator();
        var deposit = new CreateSavingsMovementCommand("key", Day, Person.Vitaliy, 1_000m, null);

        Assert.True(validator.Validate(deposit).IsValid);
        Assert.True(validator.Validate(deposit with { Amount = -120.50m }).IsValid);
        Assert.False(validator.Validate(deposit with { Amount = 0m }).IsValid);
        Assert.False(validator.Validate(deposit with { IdempotencyKey = "" }).IsValid);
    }
}

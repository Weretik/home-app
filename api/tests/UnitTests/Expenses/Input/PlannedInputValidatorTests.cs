using Expenses.Application.Features.Planned.Commands.CreatePlannedExpense;
using Expenses.Application.Features.Planned.Commands.RecordPlannedExpense;
using Expenses.Application.Features.Planned.Commands.UpdatePlannedExpense;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Planning;

namespace UnitTests.Expenses.Input;

/// <summary>The shape checks of the plan writes (SDD 005 SC-FC-003, R-FC-004).</summary>
public sealed class PlannedInputValidatorTests
{
    private static readonly Guid Category = Guid.NewGuid();

    [Fact]
    public void A_Plan_NeedsAKey_ACalendarMonth_AName_ACategory_AndAPositiveAmount()
    {
        var validator = new CreatePlannedExpenseValidator();
        var valid = new CreatePlannedExpenseCommand("key", 2026, 12, "Подарки", Category, Payer.Shared, 3_000m);

        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(validator.Validate(valid with { IdempotencyKey = "" }).IsValid);
        Assert.False(validator.Validate(valid with { Month = 13 }).IsValid);
        Assert.False(validator.Validate(valid with { Year = 1999 }).IsValid);
        Assert.False(validator.Validate(valid with { Name = "" }).IsValid);
        Assert.False(validator.Validate(valid with { Name = new string('x', PlannedExpense.MaxNameLength + 1) }).IsValid);
        Assert.False(validator.Validate(valid with { CategoryId = Guid.Empty }).IsValid);
        Assert.False(validator.Validate(valid with { Amount = 0m }).IsValid);
    }

    [Fact]
    public void A_PlanEdit_AndARecord_NeedThePlansVersion()
    {
        var update = new UpdatePlannedExpenseValidator();
        var record = new RecordPlannedExpenseValidator();
        var edit = new UpdatePlannedExpenseCommand(Guid.NewGuid(), 4, 2026, 12, "Подарки", Category, Payer.Maria, 1m);
        var recording = new RecordPlannedExpenseCommand(Guid.NewGuid(), 4, new DateOnly(2026, 12, 20), PaymentMethod.Card, "торт");

        Assert.True(update.Validate(edit).IsValid);
        Assert.False(update.Validate(edit with { Version = 0 }).IsValid);
        Assert.True(record.Validate(recording).IsValid);
        Assert.False(record.Validate(recording with { Version = 0 }).IsValid);
        Assert.False(record.Validate(recording with { PaymentMethod = (PaymentMethod)9 }).IsValid);
    }
}

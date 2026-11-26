using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Planned.Commands.RecordPlannedExpense;

public sealed class RecordPlannedExpenseHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<RecordPlannedExpenseCommand, Result<CreatedEntry>>
{
    public async ValueTask<Result<CreatedEntry>> Handle(RecordPlannedExpenseCommand c, CancellationToken cancellationToken)
    {
        var plan = await db.PlannedExpenses.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (plan is null)
            return EntryErrors.NotFound("planned_expense", c.Id);
        if (plan.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        var expense = plan.Record(c.Date, c.PaymentMethod, c.Comment, clock.GetUtcNow());
        if (expense.IsFailure)
            return expense.Error!;

        // One save, one transaction: the expense appears exactly when the plan goes.
        db.Expenses.Add(expense.Value);
        db.PlannedExpenses.Remove(plan);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return EntryErrors.ChangedMeanwhile;
        }

        return new CreatedEntry(expense.Value.Id);
    }
}

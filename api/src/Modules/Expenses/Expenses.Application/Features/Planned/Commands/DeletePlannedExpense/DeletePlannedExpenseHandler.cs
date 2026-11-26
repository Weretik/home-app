using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Planned.Commands.DeletePlannedExpense;

public sealed class DeletePlannedExpenseHandler(IExpensesDbContext db) : ICommandHandler<DeletePlannedExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(DeletePlannedExpenseCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.PlannedExpenses.Where(x => x.Id == c.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? EntryErrors.NotFound("planned_expense", c.Id) : Result.Success();
    }
}

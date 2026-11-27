using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Expenses.Domain.Entries.Identifiers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseHandler(IExpensesDbContext db) : ICommandHandler<DeleteExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteExpenseCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.Expenses.Where(x => x.Id == new ExpenseId(c.Id)).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? EntryErrors.NotFound("expense", c.Id) : Result.Success();
    }
}

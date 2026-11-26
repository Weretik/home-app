using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Expenses.Commands.DeleteExpense;

public sealed class DeleteExpenseHandler(IExpensesDbContext db) : ICommandHandler<DeleteExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteExpenseCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.Expenses.Where(x => x.Id == c.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? EntryErrors.NotFound("expense", c.Id) : Result.Success();
    }
}

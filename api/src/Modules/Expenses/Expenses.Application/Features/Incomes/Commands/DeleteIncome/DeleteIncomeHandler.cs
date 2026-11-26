using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Incomes.Commands.DeleteIncome;

public sealed class DeleteIncomeHandler(IExpensesDbContext db) : ICommandHandler<DeleteIncomeCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteIncomeCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.Incomes.Where(x => x.Id == c.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? EntryErrors.NotFound("income", c.Id) : Result.Success();
    }
}

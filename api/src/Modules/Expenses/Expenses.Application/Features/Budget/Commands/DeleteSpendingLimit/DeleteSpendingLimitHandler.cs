using BuildingBlocks.Domain.Results;
using Expenses.Application;
using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Identifiers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Commands.DeleteSpendingLimit;

public sealed class DeleteSpendingLimitHandler(IExpensesDbContext db) : ICommandHandler<DeleteSpendingLimitCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSpendingLimitCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.SpendingLimits.Where(x => x.Id == new SpendingLimitId(c.Id)).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? BudgetErrors.LimitNotFound(new SpendingLimitId(c.Id)) : Result.Success();
    }
}

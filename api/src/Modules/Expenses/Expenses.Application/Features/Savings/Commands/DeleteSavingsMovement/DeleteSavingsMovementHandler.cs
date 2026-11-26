using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Savings.Commands.DeleteSavingsMovement;

public sealed class DeleteSavingsMovementHandler(IExpensesDbContext db) : ICommandHandler<DeleteSavingsMovementCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSavingsMovementCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.SavingsMovements.Where(x => x.Id == c.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? EntryErrors.NotFound("savings_movement", c.Id) : Result.Success();
    }
}

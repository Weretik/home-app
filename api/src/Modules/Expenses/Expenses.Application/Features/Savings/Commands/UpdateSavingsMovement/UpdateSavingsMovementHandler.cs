using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Savings;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Savings.Commands.UpdateSavingsMovement;

public sealed class UpdateSavingsMovementHandler(IExpensesDbContext db) : ICommandHandler<UpdateSavingsMovementCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateSavingsMovementCommand c, CancellationToken cancellationToken)
    {
        var entry = await db.SavingsMovements.FirstOrDefaultAsync(x => x.Id == new SavingsMovementId(c.Id), cancellationToken);
        if (entry is null)
            return EntryErrors.NotFound("savings_movement", c.Id);
        if (entry.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        var updated = entry.Update(c.Date, c.Person, c.Amount, c.Comment);
        if (updated.IsFailure)
            return updated;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return EntryErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

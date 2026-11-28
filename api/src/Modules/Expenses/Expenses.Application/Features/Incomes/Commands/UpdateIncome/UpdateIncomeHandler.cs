using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Expenses.Domain.Entries.Identifiers;
using Expenses.Domain.Parties;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Incomes.Commands.UpdateIncome;

public sealed class UpdateIncomeHandler(IExpensesDbContext db) : ICommandHandler<UpdateIncomeCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateIncomeCommand c, CancellationToken cancellationToken)
    {
        var entry = await db.Incomes.FirstOrDefaultAsync(x => x.Id == new IncomeId(c.Id), cancellationToken);
        if (entry is null)
            return EntryErrors.NotFound("income", c.Id);
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

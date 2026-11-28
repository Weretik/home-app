using BuildingBlocks.Domain.Results;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Entries.Identifiers;
using Expenses.Domain.Parties;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Expenses.Commands.UpdateExpense;

public sealed class UpdateExpenseHandler(IExpensesDbContext db) : ICommandHandler<UpdateExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateExpenseCommand c, CancellationToken cancellationToken)
    {
        var entry = await db.Expenses.FirstOrDefaultAsync(x => x.Id == new ExpenseId(c.Id), cancellationToken);
        if (entry is null)
            return EntryErrors.NotFound("expense", c.Id);
        if (entry.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        if (!await db.Categories.AnyAsync(x => x.Id == new CategoryId(c.CategoryId), cancellationToken))
            return CategoryErrors.NotFoundById(new CategoryId(c.CategoryId));

        var updated = entry.Update(c.Date, c.Payer, new CategoryId(c.CategoryId), c.Amount, c.PaymentMethod, c.Comment, c.OutsideLimits);
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

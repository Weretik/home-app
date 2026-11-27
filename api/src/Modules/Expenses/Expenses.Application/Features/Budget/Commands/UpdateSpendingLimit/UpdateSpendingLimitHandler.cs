using Expenses.Application;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;

public sealed class UpdateSpendingLimitHandler(IExpensesDbContext db, TimeProvider clock) : ICommandHandler<UpdateSpendingLimitCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateSpendingLimitCommand c, CancellationToken cancellationToken)
    {
        var limit = await db.SpendingLimits.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (limit is null)
            return BudgetErrors.LimitNotFound(c.Id);
        if (limit.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        if (await BudgetCategories.FirstMissingAsync(db, c.CategoryIds, cancellationToken) is { } missing)
            return CategoryErrors.NotFoundById(missing);

        var updated = limit.Update(c.Name, c.Period, c.Amount, c.CategoryIds, limit.SortOrder, c.AmountFrom, clock.TodayInKyiv());
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

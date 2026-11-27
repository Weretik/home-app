using BuildingBlocks.Domain.Results;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Planning;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Planned.Commands.UpdatePlannedExpense;

public sealed class UpdatePlannedExpenseHandler(IExpensesDbContext db) : ICommandHandler<UpdatePlannedExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(UpdatePlannedExpenseCommand c, CancellationToken cancellationToken)
    {
        var plan = await db.PlannedExpenses.FirstOrDefaultAsync(x => x.Id == new PlannedExpenseId(c.Id), cancellationToken);
        if (plan is null)
            return EntryErrors.NotFound("planned_expense", c.Id);
        if (plan.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        if (!await db.Categories.AnyAsync(x => x.Id == new CategoryId(c.CategoryId), cancellationToken))
            return CategoryErrors.NotFoundById(new CategoryId(c.CategoryId));

        var updated = plan.Update(new DateOnly(c.Year, c.Month, 1), c.Name, new CategoryId(c.CategoryId), c.Payer, c.Amount);
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

using BuildingBlocks.Domain.Results;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Planned.Commands.UpdatePlannedExpense;

public sealed class UpdatePlannedExpenseHandler(IExpensesDbContext db) : ICommandHandler<UpdatePlannedExpenseCommand, Result>
{
    public async ValueTask<Result> Handle(UpdatePlannedExpenseCommand c, CancellationToken cancellationToken)
    {
        var plan = await db.PlannedExpenses.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (plan is null)
            return EntryErrors.NotFound("planned_expense", c.Id);
        if (plan.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        if (!await db.Categories.AnyAsync(x => x.Id == c.CategoryId, cancellationToken))
            return CategoryErrors.NotFoundById(c.CategoryId);

        var updated = plan.Update(new DateOnly(c.Year, c.Month, 1), c.Name, c.CategoryId, c.Payer, c.Amount);
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

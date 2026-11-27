using Expenses.Application;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Expenses.Domain.Entries;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;

public sealed class UpdateBudgetSettingsHandler(IExpensesDbContext db, TimeProvider clock) : ICommandHandler<UpdateBudgetSettingsCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateBudgetSettingsCommand c, CancellationToken cancellationToken)
    {
        var settings = await db.BudgetSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
            return BudgetErrors.SettingsNotFound;
        if (settings.Version != c.Version)
            return EntryErrors.ChangedMeanwhile;

        var updated = settings.Update(
            c.MonthlyLimit, c.SavingsGoal, c.LimitsFrom ?? settings.LimitsFrom, c.MonthlyLimitFrom, clock.TodayInKyiv());
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

using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Items;
using Shopping.Domain.Settings;

namespace Shopping.Application.Features.Settings.Commands.UpdateSettings;

public sealed class UpdateSettingsHandler(IShoppingDbContext db) : ICommandHandler<UpdateSettingsCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateSettingsCommand c, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.FirstOrDefaultAsync(cancellationToken);
        if (settings is null)
            return ShoppingSettingsErrors.Missing;
        if (settings.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;

        var updated = settings.Update(c.CartKeepHours, c.FrequentDays);
        if (updated.IsFailure)
            return updated;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

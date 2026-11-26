using BuildingBlocks.Domain.Results;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.ChangeGiftStatus;

public sealed class ChangeGiftStatusHandler(IGiftsDbContext db, TimeProvider clock)
    : ICommandHandler<ChangeGiftStatusCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeGiftStatusCommand c, CancellationToken cancellationToken)
    {
        var gift = await db.Gifts.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (gift is null)
            return GiftErrors.NotFound(c.Id);
        if (gift.Version != c.Version)
            return GiftErrors.ChangedMeanwhile;

        var changed = gift.ChangeStatus(c.Status, c.GiftedOn, HouseholdDay.Today(clock));
        if (changed.IsFailure)
            return changed;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return GiftErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

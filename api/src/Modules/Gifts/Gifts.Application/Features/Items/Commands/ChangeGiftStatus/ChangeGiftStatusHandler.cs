using BuildingBlocks.Domain.Results;
using Gifts.Application.Purchases;
using Gifts.Domain.Wishlist;
using Gifts.Domain.Wishlist.Identifiers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.ChangeGiftStatus;

public sealed class ChangeGiftStatusHandler(IGiftsDbContext db, TimeProvider clock, GiftLinks links)
    : ICommandHandler<ChangeGiftStatusCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeGiftStatusCommand c, CancellationToken cancellationToken)
    {
        var gift = await db.Gifts.FirstOrDefaultAsync(x => x.Id == new GiftId(c.Id), cancellationToken);
        if (gift is null)
            return GiftErrors.NotFound(new GiftId(c.Id));
        if (gift.Version != c.Version)
            return GiftErrors.ChangedMeanwhile;

        var before = PurchasePlan.Of(gift);
        var changed = gift.ChangeStatus(c.Status, c.GiftedOn, HouseholdDay.Today(clock));
        if (changed.IsFailure)
            return changed;

        PurchasePlan.Publish(db, before, gift, links);

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

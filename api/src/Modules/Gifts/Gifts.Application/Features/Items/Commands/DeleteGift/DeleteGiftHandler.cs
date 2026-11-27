using BuildingBlocks.Domain.Results;
using Gifts.Contracts;
using Gifts.Domain.Wishlist;
using Gifts.Domain.Wishlist.Identifiers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.DeleteGift;

/// <summary>
/// Removes the gift with its variants; a planned gift also publishes <see cref="GiftPurchaseClosed"/> in the same save,
/// so its to-do is cancelled (spec gifts/002, R-003).
/// </summary>
public sealed class DeleteGiftHandler(IGiftsDbContext db) : ICommandHandler<DeleteGiftCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteGiftCommand c, CancellationToken cancellationToken)
    {
        var gift = await db.Gifts.FirstOrDefaultAsync(x => x.Id == new GiftId(c.Id), cancellationToken);
        if (gift is null)
            return GiftErrors.NotFound(new GiftId(c.Id));

        if (gift.IsPurchasePlanned)
            db.Publish(new GiftPurchaseClosed(gift.Id.Value, GiftPurchaseOutcome.Deleted));
        db.Gifts.Remove(gift);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone deleted it between the read and the save.
            return GiftErrors.NotFound(new GiftId(c.Id));
        }

        return Result.Success();
    }
}

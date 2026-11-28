using BuildingBlocks.Domain.Results;
using Gifts.Application.Purchases;
using Gifts.Domain.Categories;
using Gifts.Domain.Wishlist;
using Gifts.Domain.Wishlist.Identifiers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.UpdateGift;

public sealed class UpdateGiftHandler(IGiftsDbContext db, TimeProvider clock, GiftLinks links) : ICommandHandler<UpdateGiftCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateGiftCommand c, CancellationToken cancellationToken)
    {
        var gift = await db.Gifts.FirstOrDefaultAsync(x => x.Id == new GiftId(c.Id), cancellationToken);
        if (gift is null)
            return GiftErrors.NotFound(new GiftId(c.Id));
        if (gift.Version != c.Version)
            return GiftErrors.ChangedMeanwhile;

        if (!await db.Categories.AnyAsync(x => x.Id == new CategoryId(c.CategoryId), cancellationToken))
            return CategoryErrors.NotFound(new CategoryId(c.CategoryId));

        var before = PurchasePlan.Of(gift);
        var updated = gift.Update(
            c.Recipient, c.Name, new CategoryId(c.CategoryId), c.Priority, c.Price, c.Link, c.Occasion, c.OccasionDate, c.Notes, c.Variants,
            clock.GetUtcNow());
        if (updated.IsFailure)
            return updated;
        var planned = gift.PlanPurchase(c.Giver, c.BuyBy);
        if (planned.IsFailure)
            return planned;

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

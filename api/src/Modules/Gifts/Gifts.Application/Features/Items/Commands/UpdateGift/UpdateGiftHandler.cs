using BuildingBlocks.Domain.Results;
using Gifts.Domain.Categories;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.UpdateGift;

public sealed class UpdateGiftHandler(IGiftsDbContext db, TimeProvider clock) : ICommandHandler<UpdateGiftCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateGiftCommand c, CancellationToken cancellationToken)
    {
        var gift = await db.Gifts.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (gift is null)
            return GiftErrors.NotFound(c.Id);
        if (gift.Version != c.Version)
            return GiftErrors.ChangedMeanwhile;

        if (!await db.Categories.AnyAsync(x => x.Id == c.CategoryId, cancellationToken))
            return CategoryErrors.NotFound(c.CategoryId);

        var updated = gift.Update(
            c.Recipient, c.Name, c.CategoryId, c.Priority, c.Price, c.Link, c.Occasion, c.OccasionDate, c.Notes, c.Variants,
            clock.GetUtcNow());
        if (updated.IsFailure)
            return updated;

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

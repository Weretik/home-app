using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Gifts.Application.Idempotency;
using Gifts.Application.Purchases;
using Gifts.Domain.Categories;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.CreateGift;

public sealed class CreateGiftHandler(IGiftsDbContext db, TimeProvider clock, GiftLinks links)
    : ICommandHandler<CreateGiftCommand, Result<CreatedGiftsEntry>>
{
    private const string Kind = "gift";

    public async ValueTask<Result<CreatedGiftsEntry>> Handle(CreateGiftCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[]
        {
            c.Recipient, c.Name, c.CategoryId, c.Priority, c.Price, c.Link, c.Occasion, c.OccasionDate, c.Notes, c.Variants, c.Giver, c.BuyBy,
        });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (!await db.Categories.AnyAsync(x => x.Id == new CategoryId(c.CategoryId), cancellationToken))
            return CategoryErrors.NotFound(new CategoryId(c.CategoryId));

        var now = clock.GetUtcNow();
        var created = Gift.Create(
            c.Recipient, c.Name, new CategoryId(c.CategoryId), c.Priority, c.Price, c.Link, c.Occasion, c.OccasionDate, c.Notes, c.Variants, now);
        if (created.IsFailure)
            return created.Error!;
        var planned = created.Value.PlanPurchase(c.Giver, c.BuyBy);
        if (planned.IsFailure)
            return planned.Error!;

        db.Gifts.Add(created.Value);
        PurchasePlan.Publish(db, null, created.Value, links);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id.Value, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of the same request saved first: answer like a repeat.
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? IdempotentCreates.KeyReused;
        }

        return new CreatedGiftsEntry(created.Value.Id.Value);
    }
}

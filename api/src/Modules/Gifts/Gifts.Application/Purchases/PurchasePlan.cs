using Gifts.Contracts;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Purchases;

/// <summary>
/// What the giver's to-do is built from: a gift that is wanted and has a giver and a buy-by date (spec gifts/002).
/// A handler takes it before a change and asks <see cref="Change"/> which event, if any, the change needs.
/// </summary>
public sealed record PurchasePlan(string Name, Recipient Recipient, Recipient Giver, DateOnly BuyBy)
{
    public static PurchasePlan? Of(Gift gift) =>
        gift.IsPurchasePlanned ? new PurchasePlan(gift.Name, gift.Recipient, gift.Giver!.Value, gift.BuyBy!.Value) : null;

    /// <summary>
    /// The event for a gift that was <paramref name="before"/> and is now <paramref name="gift"/>: planned (new or
    /// changed), closed (with why), or none when nothing the to-do shows has changed.
    /// </summary>
    public static object? Change(PurchasePlan? before, Gift gift, GiftLinks links)
    {
        var after = Of(gift);
        if (after is not null)
        {
            return after == before
                ? null
                : new GiftPurchasePlanned(gift.Id, after.Name, after.Recipient.ToParty(), after.Giver.ToParty(), after.BuyBy, links.For(gift.Id));
        }

        if (before is null)
            return null;

        var outcome = gift.Status switch
        {
            GiftStatus.Gifted => GiftPurchaseOutcome.Gifted,
            GiftStatus.Dropped => GiftPurchaseOutcome.Dropped,
            _ => GiftPurchaseOutcome.Unplanned,
        };
        return new GiftPurchaseClosed(gift.Id, outcome);
    }

    /// <summary>Publishes the event of <see cref="Change"/>, if there is one.</summary>
    public static void Publish(IGiftsDbContext db, PurchasePlan? before, Gift gift, GiftLinks links)
    {
        if (Change(before, gift, links) is { } message)
            db.Publish(message);
    }
}

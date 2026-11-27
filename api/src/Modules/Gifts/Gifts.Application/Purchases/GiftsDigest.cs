using Gifts.Contracts;
using Gifts.Domain.Wishlist;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Purchases;

/// <summary>The Gifts side of <see cref="IGiftsDigest"/>: planned purchases due in a range, read without tracking.</summary>
public sealed class GiftsDigest(IGiftsDbContext db, GiftLinks links) : IGiftsDigest
{
    public async Task<IReadOnlyList<GiftPurchase>> ListPurchasesDueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var rows = await db.Gifts.AsNoTracking()
            .Where(g => g.Status == GiftStatus.Wanted && g.Giver != null && g.BuyBy >= from && g.BuyBy <= to)
            .OrderBy(g => g.BuyBy)
            .ThenBy(g => g.Name)
            .Select(g => new { g.Id, g.Name, g.Recipient, Giver = g.Giver!.Value, BuyBy = g.BuyBy!.Value })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(g => new GiftPurchase(g.Id.Value, g.Name, g.Recipient.ToParty(), g.Giver.ToParty(), g.BuyBy, links.For(g.Id.Value)))];
    }
}

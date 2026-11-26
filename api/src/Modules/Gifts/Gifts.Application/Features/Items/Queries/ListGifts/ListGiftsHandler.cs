using BuildingBlocks.Domain.Results;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Queries.ListGifts;

/// <summary>
/// One query: the gifts of the list, their category names and their variants. The active list is ordered by priority
/// (High first), then the nearest occasion date (none last), then creation; the archive by the gifted day (newest
/// first, none last), then the newest created.
/// </summary>
public sealed class ListGiftsHandler(IGiftsDbContext db) : IQueryHandler<ListGiftsQuery, Result<IReadOnlyList<GiftItem>>>
{
    public async ValueTask<Result<IReadOnlyList<GiftItem>>> Handle(ListGiftsQuery query, CancellationToken cancellationToken)
    {
        var gifts = query.Archived
            ? db.Gifts.AsNoTracking().Where(g => g.Status != GiftStatus.Wanted)
            : db.Gifts.AsNoTracking().Where(g => g.Status == GiftStatus.Wanted);
        var joined = gifts.Join(db.Categories, g => g.CategoryId, c => c.Id, (g, c) => new { Gift = g, CategoryName = c.Name });

        var ordered = query.Archived
            ? joined
                .OrderByDescending(x => x.Gift.GiftedOn != null)
                .ThenByDescending(x => x.Gift.GiftedOn)
                .ThenByDescending(x => x.Gift.CreatedAtUtc)
            : joined
                .OrderBy(x => x.Gift.Priority == Priority.High ? 0 : x.Gift.Priority == Priority.Normal ? 1 : 2)
                .ThenBy(x => x.Gift.OccasionDate == null)
                .ThenBy(x => x.Gift.OccasionDate)
                .ThenBy(x => x.Gift.CreatedAtUtc);

        var items = await ordered
            .Select(x => new GiftItem(
                x.Gift.Id,
                x.Gift.Version,
                x.Gift.Recipient,
                x.Gift.Name,
                x.Gift.CategoryId,
                x.CategoryName,
                x.Gift.Priority,
                x.Gift.Price,
                x.Gift.Link,
                x.Gift.Status,
                x.Gift.GiftedOn,
                x.Gift.Occasion,
                x.Gift.OccasionDate,
                x.Gift.Notes,
                x.Gift.Variants
                    .OrderBy(v => v.SortOrder)
                    .Select(v => new GiftVariantItem(v.Id, v.Title, v.Link, v.Price))
                    .ToList(),
                x.Gift.CreatedAtUtc,
                x.Gift.Giver,
                x.Gift.BuyBy))
            .ToListAsync(cancellationToken);

        return items;
    }
}

using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Details.Queries.ListDetails;

public sealed class ListDetailsHandler(IGiftsDbContext db) : IQueryHandler<ListDetailsQuery, Result<IReadOnlyList<GiftDetailItem>>>
{
    public async ValueTask<Result<IReadOnlyList<GiftDetailItem>>> Handle(ListDetailsQuery query, CancellationToken cancellationToken) =>
        await ReadAllAsync(db, cancellationToken);

    /// <summary>
    /// Every detail, by recipient in the order of the <c>Recipient</c> enum (stored as text, so sorted after the read),
    /// then by position. Tens of rows; the summary reads them the same way.
    /// </summary>
    internal static async Task<List<GiftDetailItem>> ReadAllAsync(IGiftsDbContext db, CancellationToken cancellationToken)
    {
        var details = await db.Details
            .AsNoTracking()
            .Select(d => new GiftDetailItem(d.Id.Value, d.Version, d.Recipient, d.Label, d.Value, d.SortOrder))
            .ToListAsync(cancellationToken);

        return [.. details.OrderBy(d => d.Recipient).ThenBy(d => d.SortOrder).ThenBy(d => d.Label, StringComparer.Ordinal)];
    }
}

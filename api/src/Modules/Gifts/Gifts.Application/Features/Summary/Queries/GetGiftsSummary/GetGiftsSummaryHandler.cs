using BuildingBlocks.Domain.Results;
using Gifts.Application.Features.Details.Queries.ListDetails;
using Gifts.Application.Features.Summary.Report;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Summary.Queries.GetGiftsSummary;

/// <summary>
/// Reads the few columns of the wanted and gifted gifts once (a few hundred rows at most) and the details, then puts
/// the figures together in memory (<see cref="SummaryFigures"/>). Nothing is stored. "Today" for upcoming occasions is
/// the date in Kyiv.
/// </summary>
public sealed class GetGiftsSummaryHandler(IGiftsDbContext db, TimeProvider clock)
    : IQueryHandler<GetGiftsSummaryQuery, Result<GiftsSummary>>
{
    public async ValueTask<Result<GiftsSummary>> Handle(GetGiftsSummaryQuery query, CancellationToken cancellationToken)
    {
        var gifts = await db.Gifts.AsNoTracking()
            .Where(g => g.Status != GiftStatus.Dropped)
            .Select(g => new SummaryRow(
                g.Id.Value, g.Recipient, g.Name, g.Priority, g.Price, g.Link, g.Status, g.Occasion, g.OccasionDate, g.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        var details = await ListDetailsHandler.ReadAllAsync(db, cancellationToken);

        return SummaryFigures.Build(gifts, details, HouseholdDay.Today(clock));
    }
}

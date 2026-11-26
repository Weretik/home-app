using BuildingBlocks.Domain.Results;
using Gifts.Application.Features.Details.Queries.ListDetails;
using Gifts.Application.Features.Summary.Report;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Summary.Queries.GetGiftsSummary;

/// <summary>
/// Reads the few columns of the wanted and gifted gifts once (a few hundred rows at most) and the details, then puts
/// the figures together in memory. Nothing is stored. "Today" for upcoming occasions is the date in Kyiv.
/// </summary>
public sealed class GetGiftsSummaryHandler(IGiftsDbContext db, TimeProvider clock)
    : IQueryHandler<GetGiftsSummaryQuery, Result<GiftsSummary>>
{
    private const int TopCount = 5;
    private const int UpcomingCount = 5;

    public async ValueTask<Result<GiftsSummary>> Handle(GetGiftsSummaryQuery query, CancellationToken cancellationToken)
    {
        var gifts = await db.Gifts.AsNoTracking()
            .Where(g => g.Status != GiftStatus.Dropped)
            .Select(g => new { g.Id, g.Recipient, g.Name, g.Priority, g.Price, g.Link, g.Status, g.Occasion, g.OccasionDate, g.CreatedAtUtc })
            .ToListAsync(cancellationToken);
        var details = await ListDetailsHandler.ReadAllAsync(db, cancellationToken);
        var today = HouseholdDay.Today(clock);

        var wanted = gifts
            .Where(g => g.Status == GiftStatus.Wanted)
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.OccasionDate is null)
            .ThenBy(g => g.OccasionDate)
            .ThenBy(g => g.CreatedAtUtc)
            .ToList();

        var recipients = Enum.GetValues<Recipient>()
            .Select(recipient => new RecipientSummary(
                recipient,
                Count(gifts.Where(g => g.Recipient == recipient).Select(g => (g.Status, g.Priority, g.Price))),
                [.. wanted
                    .Where(g => g.Recipient == recipient && g.Priority == Priority.High)
                    .Take(TopCount)
                    .Select(g => new SummaryGift(g.Id, g.Name, g.Price, g.Link))]))
            .ToList();

        var upcoming = wanted
            .Where(g => g.OccasionDate >= today)
            .OrderBy(g => g.OccasionDate)
            .ThenBy(g => g.CreatedAtUtc)
            .Take(UpcomingCount)
            .Select(g => new UpcomingOccasion(g.Id, g.Recipient, g.Name, g.Occasion, g.OccasionDate!.Value))
            .ToList();

        return new GiftsSummary(recipients, Count(gifts.Select(g => (g.Status, g.Priority, g.Price))), upcoming, details);
    }

    private static GiftCounts Count(IEnumerable<(GiftStatus Status, Priority Priority, decimal? Price)> gifts)
    {
        var rows = gifts.ToList();
        var wanted = rows.Where(g => g.Status == GiftStatus.Wanted).ToList();
        var gifted = rows.Where(g => g.Status == GiftStatus.Gifted).ToList();
        return new GiftCounts(
            wanted.Count,
            wanted.Sum(g => g.Price ?? 0),
            wanted.Count(g => g.Price is null),
            wanted.Count(g => g.Priority == Priority.High),
            gifted.Count,
            gifted.Sum(g => g.Price ?? 0));
    }
}

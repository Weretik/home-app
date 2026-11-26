using Gifts.Application.Features.Details.Queries.ListDetails;
using Gifts.Application.Features.Summary.Report;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Features.Summary.Queries.GetGiftsSummary;

/// <summary>
/// The arithmetic of the gifts summary over the rows already read: the counts and sums per recipient and in total, the
/// 🔥 wishes and the upcoming occasions. Pure, so it is unit-tested with fixed rows and a fixed "today".
/// </summary>
internal static class SummaryFigures
{
    private const int TopCount = 5;
    private const int UpcomingCount = 5;

    public static GiftsSummary Build(IReadOnlyList<SummaryRow> gifts, IReadOnlyList<GiftDetailItem> details, DateOnly today)
    {
        var counted = gifts.Where(g => g.Status != GiftStatus.Dropped).ToList();
        var wanted = counted
            .Where(g => g.Status == GiftStatus.Wanted)
            .OrderBy(g => g.Priority)
            .ThenBy(g => g.OccasionDate is null)
            .ThenBy(g => g.OccasionDate)
            .ThenBy(g => g.CreatedAtUtc)
            .ToList();

        var recipients = Enum.GetValues<Recipient>()
            .Select(recipient => new RecipientSummary(
                recipient,
                Count(counted.Where(g => g.Recipient == recipient)),
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

        return new GiftsSummary(recipients, Count(counted), upcoming, details);
    }

    private static GiftCounts Count(IEnumerable<SummaryRow> gifts)
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

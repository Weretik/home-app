using Gifts.Application.Features.Details.Queries.ListDetails;

namespace Gifts.Application.Features.Summary.Report;

/// <summary>
/// The home page's view of the wishlist: every recipient (Маша, Виталий, Вместе, always all three, in that order), the
/// total, up to five upcoming occasions, and all details. Computed on every read.
/// </summary>
public sealed record GiftsSummary(
    IReadOnlyList<RecipientSummary> Recipients,
    GiftCounts Total,
    IReadOnlyList<UpcomingOccasion> Upcoming,
    IReadOnlyList<GiftDetailItem> Details);

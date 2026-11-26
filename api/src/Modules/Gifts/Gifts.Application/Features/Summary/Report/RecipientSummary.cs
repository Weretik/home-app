using Gifts.Domain.People;

namespace Gifts.Application.Features.Summary.Report;

/// <summary>One recipient's figures and up to five of their High-priority wanted gifts, in the active list's order.</summary>
public sealed record RecipientSummary(Recipient Recipient, GiftCounts Counts, IReadOnlyList<SummaryGift> TopWanted);

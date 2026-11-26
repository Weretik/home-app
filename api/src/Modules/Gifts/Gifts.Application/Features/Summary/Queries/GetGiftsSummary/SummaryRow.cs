using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Features.Summary.Queries.GetGiftsSummary;

/// <summary>The few columns of a wanted or gifted gift that the summary needs, read once.</summary>
internal sealed record SummaryRow(
    Guid Id,
    Recipient Recipient,
    string Name,
    Priority Priority,
    decimal? Price,
    string? Link,
    GiftStatus Status,
    string? Occasion,
    DateOnly? OccasionDate,
    DateTimeOffset CreatedAtUtc);

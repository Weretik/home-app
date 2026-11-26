using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Features.Items.Queries.ListGifts;

/// <summary>
/// A gift as the list shows it, with its category name and its variants in order; <see cref="Giver"/> and
/// <see cref="BuyBy"/> plan its purchase (spec gifts/002).
/// </summary>
public sealed record GiftItem(
    Guid Id,
    uint Version,
    Recipient Recipient,
    string Name,
    Guid CategoryId,
    string CategoryName,
    Priority Priority,
    decimal? Price,
    string? Link,
    GiftStatus Status,
    DateOnly? GiftedOn,
    string? Occasion,
    DateOnly? OccasionDate,
    string? Notes,
    IReadOnlyList<GiftVariantItem> Variants,
    DateTimeOffset CreatedAtUtc,
    Recipient? Giver,
    DateOnly? BuyBy);

namespace Gifts.Application.Features.Items.Queries.ListGifts;

/// <summary>One variant of a gift: a link with an optional title and price.</summary>
public sealed record GiftVariantItem(Guid Id, string? Title, string Link, decimal? Price);

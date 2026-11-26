namespace Gifts.Api.Endpoints.Items;

/// <summary>One variant of a gift: a link, and optionally a title and a price.</summary>
internal sealed record GiftVariantRequest(string? Title, string Link, decimal? Price);

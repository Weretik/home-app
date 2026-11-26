namespace Gifts.Domain.Wishlist;

/// <summary>One variant as a person enters it: a link, and optionally what it is and what it costs.</summary>
public sealed record VariantDraft(string? Title, string Link, decimal? Price);

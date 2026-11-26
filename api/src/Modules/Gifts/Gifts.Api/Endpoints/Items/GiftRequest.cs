using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace Gifts.Api.Endpoints.Items;

/// <summary>
/// The fields of a gift, for adding or changing one. <see cref="Variants"/> is the whole list (missing or empty means
/// none); on a change it replaces the stored one. The status is not here: it changes through its own operation.
/// </summary>
internal sealed record GiftRequest(
    Recipient Recipient,
    string Name,
    Guid CategoryId,
    Priority Priority,
    decimal? Price,
    string? Link,
    string? Occasion,
    DateOnly? OccasionDate,
    string? Notes,
    IReadOnlyList<GiftVariantRequest>? Variants)
{
    public IReadOnlyList<VariantDraft> VariantDrafts() =>
        [.. (Variants ?? []).Select(v => new VariantDraft(v?.Title, v?.Link!, v?.Price))];
}

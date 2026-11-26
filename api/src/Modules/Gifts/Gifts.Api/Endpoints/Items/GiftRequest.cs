using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace Gifts.Api.Endpoints.Items;

/// <summary>
/// The fields of a gift, for adding or changing one. <see cref="Variants"/> is the whole list (missing or empty means
/// none); on a change it replaces the stored one. <see cref="Giver"/> («Кто дарит») and <see cref="BuyBy"/> («Купить
/// до») together make the giver's to-do (spec gifts/002). The status is not here: it changes through its own operation.
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
    IReadOnlyList<GiftVariantRequest>? Variants,
    Recipient? Giver = null,
    DateOnly? BuyBy = null)
{
    public IReadOnlyList<VariantDraft> VariantDrafts() =>
        [.. (Variants ?? []).Select(v => new VariantDraft(v?.Title, v?.Link!, v?.Price))];
}

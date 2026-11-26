using BuildingBlocks.Domain.Modeling;

namespace Gifts.Domain.Wishlist;

/// <summary>
/// One more place or model of the same gift, with its own link (the sheet kept them in the notes). It belongs to its
/// <see cref="Gift"/>: it is created, replaced and deleted only through the gift.
/// </summary>
public sealed class GiftVariant : Entity<Guid>
{
    private GiftVariant()
    {
    }

    public string? Title { get; private set; }

    public string Link { get; private set; } = string.Empty;

    public decimal? Price { get; private set; }

    /// <summary>The position in the gift's list, from 0, in the order the person entered them.</summary>
    public int SortOrder { get; private set; }

    internal static GiftVariant From(VariantDraft draft, int position, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(now),
        Title = Gift.Normalize(draft.Title),
        Link = draft.Link.Trim(),
        Price = draft.Price,
        SortOrder = position,
    };
}

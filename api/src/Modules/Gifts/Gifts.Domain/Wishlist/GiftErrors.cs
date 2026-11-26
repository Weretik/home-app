using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.Wishlist;

/// <summary>The rules a gift can break, with stable codes for the client.</summary>
public static class GiftErrors
{
    public static readonly Error NameRequired =
        Error.Validation("gifts.name_required", "The gift needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("gifts.name_too_long", $"The name is longer than {Gift.MaxNameLength} characters.", "name");

    public static readonly Error PriceInvalid = Error.Validation(
        "gifts.price_invalid",
        $"A price is more than 0 and at most {Gift.MaxPrice:0} hryvnias, with at most two digits after the point; leave it empty when unknown.",
        "price");

    public static readonly Error LinkInvalid = Error.Validation(
        "gifts.link_invalid",
        $"A link is a full http or https address of at most {Gift.MaxLinkLength} characters.",
        "link");

    public static readonly Error OccasionTooLong =
        Error.Validation("gifts.occasion_too_long", $"The occasion is longer than {Gift.MaxOccasionLength} characters.", "occasion");

    public static readonly Error OccasionDateOutOfRange =
        Error.Validation("gifts.occasion_date_out_of_range", "The occasion date must be between 2000-01-01 and 2100-12-31.", "occasionDate");

    public static readonly Error BuyByOutOfRange =
        Error.Validation("gifts.buy_by_out_of_range", "The buy-by date must be between 2000-01-01 and 2100-12-31.", "buyBy");

    public static readonly Error NotesTooLong =
        Error.Validation("gifts.notes_too_long", $"The notes are longer than {Gift.MaxNotesLength} characters.", "notes");

    public static readonly Error TooManyVariants =
        Error.Validation("gifts.too_many_variants", $"A gift has at most {Gift.MaxVariants} variants.", "variants");

    public static readonly Error VariantInvalid = Error.Validation(
        "gifts.variant_invalid",
        $"Each variant needs a full http or https link (at most {Gift.MaxLinkLength} characters), a title of at most {Gift.MaxNameLength} characters, and a price like the gift's.",
        "variants");

    public static readonly Error GiftedOnOutOfRange =
        Error.Validation("gifts.gifted_on_out_of_range", "The gifted day must be between 2000-01-01 and 2100-12-31.", "giftedOn");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "gifts.changed_meanwhile", "Someone changed or deleted this in the meantime; reload and try again.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("gifts.gift_not_found", $"There is no gift {id}.");
}

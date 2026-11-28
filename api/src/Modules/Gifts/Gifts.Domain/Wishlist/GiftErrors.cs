using BuildingBlocks.Domain.Results;
using Gifts.Domain.Wishlist.Identifiers;

namespace Gifts.Domain.Wishlist;

/// <summary>The rules a gift can break, with stable codes for the client.</summary>
public static class GiftErrors
{
    public static readonly Error NameRequired =
        Error.Validation("gifts.name_required", "Подарунку потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("gifts.name_too_long", $"Назва задовга: не більше {Gift.MaxNameLength} символів.", "name");

    public static readonly Error PriceInvalid = Error.Validation(
        "gifts.price_invalid",
        $"Ціна — більше 0 і не більше {Gift.MaxPrice:0} гривень, не більше двох знаків після коми; якщо ціна невідома, залиште поле порожнім.",
        "price");

    public static readonly Error LinkInvalid = Error.Validation(
        "gifts.link_invalid",
        $"Посилання — повна адреса http або https, не більше {Gift.MaxLinkLength} символів.",
        "link");

    public static readonly Error OccasionTooLong =
        Error.Validation("gifts.occasion_too_long", $"Привід задовгий: не більше {Gift.MaxOccasionLength} символів.", "occasion");

    public static readonly Error OccasionDateOutOfRange =
        Error.Validation("gifts.occasion_date_out_of_range", "Дата події має бути між 2000-01-01 і 2100-12-31.", "occasionDate");

    public static readonly Error BuyByOutOfRange =
        Error.Validation("gifts.buy_by_out_of_range", "Дата «Купити до» має бути між 2000-01-01 і 2100-12-31.", "buyBy");

    public static readonly Error NotesTooLong =
        Error.Validation("gifts.notes_too_long", $"Нотатки задовгі: не більше {Gift.MaxNotesLength} символів.", "notes");

    public static readonly Error TooManyVariants =
        Error.Validation("gifts.too_many_variants", $"У подарунка не більше {Gift.MaxVariants} варіантів.", "variants");

    public static readonly Error VariantInvalid = Error.Validation(
        "gifts.variant_invalid",
        $"Кожному варіанту потрібне повне посилання http або https (не більше {Gift.MaxLinkLength} символів), назва не більше {Gift.MaxNameLength} символів і ціна за тими ж правилами, що й у подарунка.",
        "variants");

    public static readonly Error GiftedOnOutOfRange =
        Error.Validation("gifts.gifted_on_out_of_range", "День дарування має бути між 2000-01-01 і 2100-12-31.", "giftedOn");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "gifts.changed_meanwhile", "Тим часом це хтось змінив або видалив; оновіть сторінку й спробуйте ще раз.");

    public static Error NotFound(GiftId id) =>
        Error.NotFound("gifts.gift_not_found", $"Подарунка {id} немає.");
}

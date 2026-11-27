using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Items;

public static class ShoppingItemErrors
{
    public static readonly Error NameRequired =
        Error.Validation("shopping.item_name_required", "Товару потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("shopping.item_name_too_long", $"Назва товару задовга: не більше {ShoppingItem.MaxNameLength} символів.", "name");

    public static readonly Error QuantityTooLong =
        Error.Validation("shopping.quantity_too_long", $"Кількість задовга: не більше {ShoppingItem.MaxQuantityLength} символів.", "quantity");

    public static readonly Error NoteTooLong =
        Error.Validation("shopping.note_too_long", $"Примітка задовга: не більше {ShoppingItem.MaxNoteLength} символів.", "note");

    public static readonly Error NothingToAdd =
        Error.Validation("shopping.nothing_to_add", "У тексті немає жодного товару.", "text");

    public static readonly Error TooManyItems =
        Error.Validation("shopping.too_many_items", $"За раз можна додати не більше {ShoppingItem.MaxItemsPerAdd} товарів.", "text");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "shopping.changed_meanwhile", "Тим часом це хтось змінив або видалив; оновіть сторінку й спробуйте ще раз.");

    /// <summary>A missing item, and an item that is history (it can no longer change), answer the same.</summary>
    public static Error NotFound(Guid id) =>
        Error.NotFound("shopping.item_not_found", $"Товару {id} немає в списку.");
}

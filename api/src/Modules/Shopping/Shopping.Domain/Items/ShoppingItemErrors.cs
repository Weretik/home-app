using BuildingBlocks.Domain.Results;

namespace Shopping.Domain.Items;

public static class ShoppingItemErrors
{
    public static readonly Error NameRequired =
        Error.Validation("shopping.item_name_required", "The item needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("shopping.item_name_too_long", $"The item name is longer than {ShoppingItem.MaxNameLength} characters.", "name");

    public static readonly Error QuantityTooLong =
        Error.Validation("shopping.quantity_too_long", $"The quantity is longer than {ShoppingItem.MaxQuantityLength} characters.", "quantity");

    public static readonly Error NoteTooLong =
        Error.Validation("shopping.note_too_long", $"The note is longer than {ShoppingItem.MaxNoteLength} characters.", "note");

    public static readonly Error NothingToAdd =
        Error.Validation("shopping.nothing_to_add", "The text holds no item.", "text");

    public static readonly Error TooManyItems =
        Error.Validation("shopping.too_many_items", $"At most {ShoppingItem.MaxItemsPerAdd} items can be added at once.", "text");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "shopping.changed_meanwhile", "Someone changed or deleted this in the meantime; reload and try again.");

    /// <summary>A missing item, and an item that is history (it can no longer change), answer the same.</summary>
    public static Error NotFound(Guid id) =>
        Error.NotFound("shopping.item_not_found", $"There is no item {id} in the list.");
}

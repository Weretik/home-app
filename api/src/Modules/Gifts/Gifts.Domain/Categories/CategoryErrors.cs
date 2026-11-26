using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error NameRequired =
        Error.Validation("gifts.category_name_required", "The category needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("gifts.category_name_too_long", $"The category name is longer than {Category.MaxNameLength} characters.", "name");

    public static readonly Error NameTaken =
        Error.Conflict("gifts.category_name_taken", "There is already a category with this name.");

    public static readonly Error InUse =
        Error.Conflict("gifts.category_in_use", "Gifts use this category; move them to another category first.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("gifts.category_not_found", $"There is no gift category {id}.");
}

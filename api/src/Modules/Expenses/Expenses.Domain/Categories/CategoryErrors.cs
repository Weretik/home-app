using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error NameRequired =
        Error.Validation("expenses.category_name_required", "The category needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("expenses.category_name_too_long", $"The category name is longer than {Category.MaxNameLength} characters.", "name");

    public static Error NotFound(string name) =>
        Error.NotFound("expenses.category_not_found", $"There is no category «{name}».");
}

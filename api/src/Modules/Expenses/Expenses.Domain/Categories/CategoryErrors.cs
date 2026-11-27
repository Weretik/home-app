using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error NameRequired =
        Error.Validation("expenses.category_name_required", "Категорії потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("expenses.category_name_too_long", $"Назва категорії задовга: не більше {Category.MaxNameLength} символів.", "name");

    public static Error NotFound(string name) =>
        Error.NotFound("expenses.category_not_found", $"Категорії «{name}» немає.");

    public static Error NotFoundById(Guid id) =>
        Error.NotFound("expenses.category_not_found", $"Категорії {id} немає.");
}

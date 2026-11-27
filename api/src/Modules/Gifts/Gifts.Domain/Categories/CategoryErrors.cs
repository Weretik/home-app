using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error NameRequired =
        Error.Validation("gifts.category_name_required", "Категорії потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("gifts.category_name_too_long", $"Назва категорії задовга: не більше {Category.MaxNameLength} символів.", "name");

    public static readonly Error NameTaken =
        Error.Conflict("gifts.category_name_taken", "Категорія з такою назвою вже є.");

    public static readonly Error InUse =
        Error.Conflict("gifts.category_in_use", "Цю категорію мають подарунки; спершу перенесіть їх в іншу категорію.");

    public static Error NotFound(CategoryId id) =>
        Error.NotFound("gifts.category_not_found", $"Категорії подарунків {id} немає.");
}

using BuildingBlocks.Application.Persistence;

namespace Gifts.Application.Features.Categories;

/// <summary>Category names are unique by a database index; a save that hits it means the name is taken.</summary>
public static class CategoryNames
{
    /// <summary>The unique index on <c>gifts.categories.name</c>; Infrastructure creates it under this name.</summary>
    public const string UniqueIndex = "ux_categories_name";

    public static bool IsNameIndex(UniqueConstraintViolationException exception) =>
        exception.ConstraintName == UniqueIndex;
}

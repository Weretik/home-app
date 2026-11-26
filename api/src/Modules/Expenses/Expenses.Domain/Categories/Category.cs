using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Categories;

/// <summary>
/// What an expense was for. Categories are data, not a fixed list: the family renames, adds and merges them, as
/// they did in the old «Справочник» sheet. The name is unique and is shown in Russian as typed.
/// </summary>
public sealed class Category : Entity<Guid>, IAggregateRoot
{
    public const int MaxNameLength = 100;

    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The position in lists and pickers, as in the old sheet.</summary>
    public int SortOrder { get; private set; }

    public static Result<Category> Create(Guid id, string name, int sortOrder)
    {
        var check = CheckName(name);
        if (check.IsFailure)
            return check.Error!;

        return new Category { Id = id, Name = name.Trim(), SortOrder = sortOrder };
    }

    private static Result CheckName(string name) =>
        Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), CategoryErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, CategoryErrors.NameTooLong));
}

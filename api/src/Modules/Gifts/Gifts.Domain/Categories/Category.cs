using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.Categories;

/// <summary>
/// What kind of gift it is («Техника», «Украшения» …). Categories are data the family edits in settings; the name is
/// unique and shown as typed.
/// </summary>
public sealed class Category : Entity<Guid>, IAggregateRoot
{
    public const int MaxNameLength = 100;

    private Category()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The position in lists and pickers, smallest first.</summary>
    public int SortOrder { get; private set; }

    public static Result<Category> Create(string name, int sortOrder, DateTimeOffset now)
    {
        var category = new Category { Id = Guid.CreateVersion7(now), SortOrder = sortOrder };
        var renamed = category.Rename(name);
        return renamed.IsFailure ? renamed.Error! : category;
    }

    public Result Rename(string name)
    {
        var check = Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), CategoryErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, CategoryErrors.NameTooLong));
        if (check.IsFailure)
            return check;

        Name = name!.Trim();
        return Result.Success();
    }
}

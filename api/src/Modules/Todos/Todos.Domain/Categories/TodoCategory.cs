using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;

namespace Todos.Domain.Categories;

/// <summary>
/// A group of tasks such as «Дом» or «Ремонт». Categories are data the family adds, renames and deletes; the name
/// is unique without regard to case. Deleting one leaves its tasks without a category.
/// </summary>
public sealed class TodoCategory : Entity<TodoCategoryId>, IAggregateRoot
{
    public const int MaxNameLength = 50;
    public const int MaxEmojiLength = 8;

    private TodoCategory()
    {
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>An optional emoji shown before the name.</summary>
    public string? Emoji { get; private set; }

    /// <summary>The position in lists and pickers; a new category goes to the end.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<TodoCategory> Create(TodoCategoryId id, string name, string? emoji, int sortOrder)
    {
        var check = Check(name, emoji);
        if (check.IsFailure)
            return check.Error!;

        return new TodoCategory { Id = id, Name = name.Trim(), Emoji = NormalizeEmoji(emoji), SortOrder = sortOrder };
    }

    public Result Rename(string name, string? emoji)
    {
        var check = Check(name, emoji);
        if (check.IsFailure)
            return check;

        Name = name.Trim();
        Emoji = NormalizeEmoji(emoji);
        return Result.Success();
    }

    private static string? NormalizeEmoji(string? emoji) =>
        string.IsNullOrWhiteSpace(emoji) ? null : emoji.Trim();

    private static Result Check(string name, string? emoji) =>
        Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name) && name.Trim().Length <= MaxNameLength, CategoryErrors.NameInvalid),
            Result.Ensure(NormalizeEmoji(emoji) is not { Length: > MaxEmojiLength }, CategoryErrors.EmojiTooLong));
}

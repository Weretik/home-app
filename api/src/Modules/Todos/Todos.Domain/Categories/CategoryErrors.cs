using BuildingBlocks.Domain.Results;

namespace Todos.Domain.Categories;

/// <summary>The rules a to-do category breaks.</summary>
public static class CategoryErrors
{
    public static readonly Error NameInvalid = Error.Validation(
        "todos.category_name_invalid", $"The category name must be 1–{TodoCategory.MaxNameLength} characters.", "name");

    public static readonly Error EmojiTooLong = Error.Validation(
        "todos.category_emoji_too_long", $"The emoji is longer than {TodoCategory.MaxEmojiLength} characters.", "emoji");

    public static readonly Error NameTaken = Error.Conflict(
        "todos.category_name_taken", "A category with this name already exists.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("todos.category_not_found", $"There is no category {id}.");
}

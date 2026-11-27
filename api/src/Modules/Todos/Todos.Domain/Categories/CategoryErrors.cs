using BuildingBlocks.Domain.Results;

namespace Todos.Domain.Categories;

/// <summary>The rules a to-do category breaks.</summary>
public static class CategoryErrors
{
    public static readonly Error NameInvalid = Error.Validation(
        "todos.category_name_invalid", $"Назва категорії має містити від 1 до {TodoCategory.MaxNameLength} символів.", "name");

    public static readonly Error EmojiTooLong = Error.Validation(
        "todos.category_emoji_too_long", $"Емодзі задовге: не більше {TodoCategory.MaxEmojiLength} символів.", "emoji");

    public static readonly Error NameTaken = Error.Conflict(
        "todos.category_name_taken", "Категорія з такою назвою вже є.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("todos.category_not_found", $"Категорії {id} немає.");
}

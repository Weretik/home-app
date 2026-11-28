using BuildingBlocks.Domain.Results;

namespace Notes.Domain.Categories;

/// <summary>The rules a note category breaks.</summary>
public static class CategoryErrors
{
    public static readonly Error NameInvalid = Error.Validation(
        "notes.category_name_invalid", $"Назва категорії має містити від 1 до {NoteCategory.MaxNameLength} символів.", "name");

    public static readonly Error EmojiTooLong = Error.Validation(
        "notes.category_emoji_too_long", $"Емодзі задовге: не більше {NoteCategory.MaxEmojiLength} символів.", "emoji");

    public static readonly Error NameTaken = Error.Conflict(
        "notes.category_name_taken", "Категорія з такою назвою вже є.");

    public static Error NotFound(NoteCategoryId id) =>
        Error.NotFound("notes.category_not_found", $"Категорії {id} немає.");
}

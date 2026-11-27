using BuildingBlocks.Domain.Results;

namespace Notes.Domain.Notes;

/// <summary>The rules a note breaks.</summary>
public static class NoteErrors
{
    public static readonly Error TitleInvalid = Error.Validation(
        "notes.title_invalid", $"Назва має містити від 1 до {Note.MaxTitleLength} символів.", "title");

    public static readonly Error DetailsTooLong = Error.Validation(
        "notes.details_too_long", $"Опис задовгий: не більше {Note.MaxDetailsLength} символів.", "details");

    public static readonly Error PlaceTooLong = Error.Validation(
        "notes.place_too_long", $"Місце чи бренд задовгі: не більше {Note.MaxPlaceLength} символів.", "place");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "notes.changed_meanwhile", "Тим часом це хтось змінив або видалив; оновіть сторінку й спробуйте ще раз.");

    public static Error NoteNotFound(Guid id) =>
        Error.NotFound("notes.note_not_found", $"Нотатки {id} немає.");
}

using BuildingBlocks.Domain.Results;

namespace Todos.Domain.Tasks;

/// <summary>The rules a task and its steps break.</summary>
public static class TodoErrors
{
    public static readonly Error TitleInvalid = Error.Validation(
        "todos.title_invalid", $"Назва має містити від 1 до {TodoTask.MaxTitleLength} символів.", "title");

    public static readonly Error NotesTooLong = Error.Validation(
        "todos.notes_too_long", $"Нотатки задовгі: не більше {TodoTask.MaxNotesLength} символів.", "notes");

    public static readonly Error DueDateOutOfRange = Error.Validation(
        "todos.due_date_out_of_range", "Термін має бути між 2000-01-01 і 2100-12-31.", "dueDate");

    public static readonly Error ImportKeyTooLong = Error.Validation(
        "todos.import_key_too_long", $"Ключ імпорту задовгий: не більше {TodoTask.MaxImportKeyLength} символів.", "importKey");

    public static readonly Error SourceAlreadySet =
        Error.Conflict("todos.source_already_set", "Завдання вже належить іншому запису.");

    public static readonly Error SourceInvalid = Error.Validation(
        "todos.source_invalid", $"Джерело — це тип (не більше {TodoTask.MaxSourceKindLength} символів) та ідентифікатор.", "source");

    public static readonly Error TooManySteps = Error.Validation(
        "todos.too_many_steps", $"У завданні не більше {TodoTask.MaxSteps} кроків.", "steps");

    public static readonly Error StepTitleInvalid = Error.Validation(
        "todos.title_invalid", $"Назва кроку має містити від 1 до {TodoTask.MaxTitleLength} символів.", "title");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "todos.changed_meanwhile", "Тим часом це хтось змінив або видалив; оновіть сторінку й спробуйте ще раз.");

    public static Error TaskNotFound(Guid id) =>
        Error.NotFound("todos.task_not_found", $"Завдання {id} немає.");

    public static Error StepNotFound(Guid id) =>
        Error.NotFound("todos.step_not_found", $"У цьому завданні немає кроку {id}.");
}

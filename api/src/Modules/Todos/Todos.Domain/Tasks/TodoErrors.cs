using BuildingBlocks.Domain.Results;

namespace Todos.Domain.Tasks;

/// <summary>The rules a task and its steps break.</summary>
public static class TodoErrors
{
    public static readonly Error TitleInvalid = Error.Validation(
        "todos.title_invalid", $"The title must be 1–{TodoTask.MaxTitleLength} characters.", "title");

    public static readonly Error NotesTooLong = Error.Validation(
        "todos.notes_too_long", $"The notes are longer than {TodoTask.MaxNotesLength} characters.", "notes");

    public static readonly Error DueDateOutOfRange = Error.Validation(
        "todos.due_date_out_of_range", "The due date must be between 2000-01-01 and 2100-12-31.", "dueDate");

    public static readonly Error ImportKeyTooLong = Error.Validation(
        "todos.import_key_too_long", $"The import key is longer than {TodoTask.MaxImportKeyLength} characters.", "importKey");

    public static readonly Error TooManySteps = Error.Validation(
        "todos.too_many_steps", $"A task has at most {TodoTask.MaxSteps} steps.", "steps");

    public static readonly Error StepTitleInvalid = Error.Validation(
        "todos.title_invalid", $"A step title must be 1–{TodoTask.MaxTitleLength} characters.", "title");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "todos.changed_meanwhile", "Someone changed or deleted this in the meantime; reload and try again.");

    public static Error TaskNotFound(Guid id) =>
        Error.NotFound("todos.task_not_found", $"There is no task {id}.");

    public static Error StepNotFound(Guid id) =>
        Error.NotFound("todos.step_not_found", $"There is no step {id} in this task.");
}

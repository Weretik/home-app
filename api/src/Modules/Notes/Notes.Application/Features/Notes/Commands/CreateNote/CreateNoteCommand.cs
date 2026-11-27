using BuildingBlocks.Domain.Results;
using Mediator;
using Notes.Application.Idempotency;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.CreateNote;

/// <summary>
/// Adds a note about one person; the author is the signed-in member, not a field of the request (R-008). A repeat
/// with the same Idempotency-Key and data returns the first id (R-010).
/// </summary>
public sealed record CreateNoteCommand(
    string IdempotencyKey,
    NotePerson About,
    NoteKind Kind,
    string Title,
    string? Details,
    string? Place,
    Guid? CategoryId,
    bool IsImportant) : ICommand<Result<NoteCreated>>;

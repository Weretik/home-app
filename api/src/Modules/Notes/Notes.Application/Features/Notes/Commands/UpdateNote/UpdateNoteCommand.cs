using BuildingBlocks.Domain.Results;
using Mediator;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.UpdateNote;

/// <summary>
/// Replaces the editable fields of a note, if nobody changed it since <see cref="Version"/> (R-009). The signed-in
/// member becomes the last editor; the author stays (R-008).
/// </summary>
public sealed record UpdateNoteCommand(
    Guid Id,
    uint Version,
    Person About,
    NoteKind Kind,
    string Title,
    string? Details,
    string? Place,
    Guid? CategoryId,
    bool IsImportant) : ICommand<Result>;

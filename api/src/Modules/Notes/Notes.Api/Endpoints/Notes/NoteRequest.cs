using Notes.Domain.Notes;

namespace Notes.Api.Endpoints.Notes;

/// <summary>
/// A note's fields, for adding or changing one. Who wrote or changed it is not a field: the server takes it from the
/// session (R-008).
/// </summary>
internal sealed record NoteRequest(
    Person About,
    NoteKind Kind,
    string Title,
    string? Details,
    string? Place,
    Guid? CategoryId,
    bool IsImportant);

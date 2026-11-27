using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Queries.ListNotes;

/// <summary>A note as the list shows it: its fields, its category's name and emoji, and its version for <c>If-Match</c>.</summary>
public sealed record NoteItem(
    Guid Id,
    Person About,
    NoteKind Kind,
    string Title,
    string? Details,
    string? Place,
    bool IsImportant,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryEmoji,
    Person? CreatedBy,
    Person? UpdatedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    uint Version);

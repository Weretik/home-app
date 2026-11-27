namespace Notes.Application.Idempotency;

/// <summary>The id of a created note or category.</summary>
public sealed record NoteCreated(Guid Id);

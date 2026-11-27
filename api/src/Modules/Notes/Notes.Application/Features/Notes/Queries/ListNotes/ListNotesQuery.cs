using BuildingBlocks.Domain.Results;
using Mediator;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Queries.ListNotes;

/// <summary>
/// The notes, optionally only about one person, of some kinds, of one category or of none, and with every word of
/// <see cref="Search"/> in the title, the description or the place (R-013). Ordered by R-012, at most
/// <see cref="ListNotesHandler.MaxNotes"/>, no paging.
/// </summary>
public sealed record ListNotesQuery(
    Person? About,
    IReadOnlyList<NoteKind> Kinds,
    Guid? CategoryId,
    bool WithoutCategory,
    string? Search) : IQuery<Result<IReadOnlyList<NoteItem>>>;

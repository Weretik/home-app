using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Queries.ListNotes;

/// <summary>A note joined with its category, if it has one; the rows the list is ordered and projected from.</summary>
public sealed class NoteWithCategory
{
    public required Note Note { get; init; }

    public NoteCategory? Category { get; init; }
}

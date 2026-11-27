using System.Linq.Expressions;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Queries.ListNotes;

/// <summary>
/// The rows of the list, in SQL: notes left-joined to their category, projected to <see cref="NoteItem"/>, in the
/// order of R-012: by category order (notes without a category last), then important first, then
/// the kind (Likes, Dislikes, Fact; stored as text, so ranked in the query), then the title without regard to case.
/// </summary>
public static class NoteRows
{
    public static IOrderedQueryable<NoteWithCategory> InListOrder(this IQueryable<NoteWithCategory> rows) =>
        rows
            .OrderBy(r => r.Category == null)
            .ThenBy(r => r.Category == null ? 0 : r.Category.SortOrder)
            .ThenBy(r => r.Category == null ? null : r.Category.Name)
            .ThenByDescending(r => r.Note.IsImportant)
            .ThenBy(r => r.Note.Kind == NoteKind.Likes ? 0 : r.Note.Kind == NoteKind.Dislikes ? 1 : 2)
            .ThenBy(r => r.Note.Title.ToLower())
            .ThenBy(r => r.Note.Id);

    public static IQueryable<NoteWithCategory> WithCategory(this IQueryable<Note> notes, IQueryable<NoteCategory> categories) =>
        notes
            .GroupJoin(categories, n => n.CategoryId, c => (Guid?)c.Id, (n, matches) => new { n, matches })
            .SelectMany(x => x.matches.DefaultIfEmpty(), (x, c) => new NoteWithCategory { Note = x.n, Category = c });

    public static readonly Expression<Func<NoteWithCategory, NoteItem>> ToItem = row => new NoteItem(
        row.Note.Id,
        row.Note.About,
        row.Note.Kind,
        row.Note.Title,
        row.Note.Details,
        row.Note.Place,
        row.Note.IsImportant,
        row.Note.CategoryId,
        row.Category == null ? null : row.Category.Name,
        row.Category == null ? null : row.Category.Emoji,
        row.Note.CreatedBy,
        row.Note.UpdatedBy,
        row.Note.CreatedAtUtc,
        row.Note.UpdatedAtUtc,
        row.Note.Version);
}

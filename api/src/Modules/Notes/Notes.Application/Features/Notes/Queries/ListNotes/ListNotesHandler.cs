using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Notes.Application.Features.Notes.Queries.ListNotes;

public sealed class ListNotesHandler(INotesDbContext db) : IQueryHandler<ListNotesQuery, Result<IReadOnlyList<NoteItem>>>
{
    /// <summary>The most notes one list returns (R-013): the family keeps hundreds, so no paging.</summary>
    public const int MaxNotes = 1000;

    public async ValueTask<Result<IReadOnlyList<NoteItem>>> Handle(ListNotesQuery query, CancellationToken cancellationToken)
    {
        var notes = db.Notes.AsNoTracking();

        if (query.About is { } about)
            notes = notes.Where(n => n.About == about);

        if (query.Kinds.Count > 0)
        {
            var kinds = query.Kinds.Distinct().ToList();
            notes = notes.Where(n => kinds.Contains(n.Kind));
        }

        if (query.CategoryId is { } categoryId)
            notes = notes.Where(n => n.CategoryId == categoryId);
        else if (query.WithoutCategory)
            notes = notes.Where(n => n.CategoryId == null);

        // Every word must occur in the title, the description or the place (R-013); hundreds of rows need no index.
        foreach (var pattern in SearchPattern.ContainsEachWord(query.Search))
        {
            notes = notes.Where(n =>
                EF.Functions.Like(n.Title.ToLower(), pattern, SearchPattern.EscapeCharacter)
                || EF.Functions.Like(n.Details!.ToLower(), pattern, SearchPattern.EscapeCharacter)
                || EF.Functions.Like(n.Place!.ToLower(), pattern, SearchPattern.EscapeCharacter));
        }

        var items = await notes
            .WithCategory(db.Categories.AsNoTracking())
            .InListOrder()
            .Select(NoteRows.ToItem)
            .Take(MaxNotes)
            .ToListAsync(cancellationToken);

        return items;
    }
}

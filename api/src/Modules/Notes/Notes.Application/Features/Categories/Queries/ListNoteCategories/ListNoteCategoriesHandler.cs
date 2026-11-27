using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Notes.Application.Features.Categories.Queries.ListNoteCategories;

public sealed class ListNoteCategoriesHandler(INotesDbContext db)
    : IQueryHandler<ListNoteCategoriesQuery, Result<IReadOnlyList<NoteCategoryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<NoteCategoryItem>>> Handle(
        ListNoteCategoriesQuery query, CancellationToken cancellationToken)
    {
        var notes = db.Notes.AsNoTracking();
        var items = await db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new NoteCategoryItem(c.Id, c.Name, c.Emoji, c.SortOrder, c.Version, notes.Count(n => n.CategoryId == c.Id)))
            .ToListAsync(cancellationToken);

        return items;
    }
}

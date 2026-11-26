using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Features.Tasks.Reading;

namespace Todos.Application.Features.Categories.Queries.ListTodoCategories;

public sealed class ListTodoCategoriesHandler(ITodosDbContext db)
    : IQueryHandler<ListTodoCategoriesQuery, Result<IReadOnlyList<TodoCategoryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<TodoCategoryItem>>> Handle(
        ListTodoCategoriesQuery query, CancellationToken cancellationToken)
    {
        var active = db.Tasks.AsNoTracking().WhereActive();
        var items = await db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new TodoCategoryItem(
                c.Id, c.Name, c.Emoji, c.SortOrder, c.Version, active.Count(t => t.CategoryId == c.Id)))
            .ToListAsync(cancellationToken);

        return items;
    }
}

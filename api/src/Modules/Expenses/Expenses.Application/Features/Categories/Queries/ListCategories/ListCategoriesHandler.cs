using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Categories.Queries.ListCategories;

public sealed class ListCategoriesHandler(IExpensesDbContext db)
    : IQueryHandler<ListCategoriesQuery, Result<IReadOnlyList<CategoryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<CategoryItem>>> Handle(
        ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var items = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryItem(c.Id, c.Name, c.SortOrder))
            .ToListAsync(cancellationToken);

        return items;
    }
}

using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Categories.Queries.ListCategories;

public sealed class ListCategoriesHandler(IGiftsDbContext db)
    : IQueryHandler<ListCategoriesQuery, Result<IReadOnlyList<GiftCategoryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<GiftCategoryItem>>> Handle(
        ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var items = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new GiftCategoryItem(c.Id.Value, c.Name, c.SortOrder))
            .ToListAsync(cancellationToken);

        return items;
    }
}

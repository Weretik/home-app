using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Departments.Queries.ListDepartments;

public sealed class ListDepartmentsHandler(IShoppingDbContext db)
    : IQueryHandler<ListDepartmentsQuery, Result<IReadOnlyList<DepartmentItem>>>
{
    public async ValueTask<Result<IReadOnlyList<DepartmentItem>>> Handle(ListDepartmentsQuery query, CancellationToken cancellationToken)
    {
        var keywords = await db.Keywords.GroupBy(k => k.DepartmentId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count, cancellationToken);
        var needed = await db.Items.Where(i => i.Status == ShoppingItemStatus.Needed).GroupBy(i => i.DepartmentId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count, cancellationToken);
        var departments = await db.Departments.AsNoTracking()
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return departments
            .Select(d => new DepartmentItem(
                d.Id, d.Name, d.Icon, d.SortOrder, d.IsFallback, keywords.GetValueOrDefault(d.Id), needed.GetValueOrDefault(d.Id), d.Version))
            .ToList();
    }
}

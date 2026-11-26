using Microsoft.EntityFrameworkCore;
using Shopping.Contracts;
using Shopping.Domain.Items;

namespace Shopping.Application;

/// <summary>The Shopping side of <see cref="IShoppingDigest"/>: what is still needed, by department, read without tracking.</summary>
public sealed class ShoppingDigest(IShoppingDbContext db) : IShoppingDigest
{
    public async Task<IReadOnlyList<NeededItem>> ListNeededAsync(CancellationToken cancellationToken) =>
        await db.Items.AsNoTracking()
            .Where(i => i.Status == ShoppingItemStatus.Needed)
            .Join(db.Departments.AsNoTracking(), i => i.DepartmentId, d => d.Id, (i, d) => new { Item = i, Department = d })
            .OrderBy(x => x.Department.SortOrder)
            .ThenByDescending(x => x.Item.Urgent)
            .ThenBy(x => x.Item.Name)
            .Select(x => new NeededItem(x.Item.Name, x.Item.Quantity, x.Item.Urgent, x.Department.Name, x.Department.Icon))
            .ToListAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;

namespace Shopping.Application.Features.Items.Views;

/// <summary>Reads items with their departments and puts them in the list's order.</summary>
public static class ItemViews
{
    /// <summary>The given items (filter them first) joined with their departments, read-only.</summary>
    public static IQueryable<ItemRow> Rows(IShoppingDbContext db, IQueryable<ShoppingItem> items) =>
        items.AsNoTracking().Join(
            db.Departments.AsNoTracking(),
            item => item.DepartmentId,
            department => department.Id,
            (item, department) => new ItemRow { Item = item, Department = department });

    public static ShoppingItemView ToView(ItemRow row) => ToView(row.Item, row.Department);

    public static ShoppingItemView ToView(ShoppingItem i, Department d) =>
        new(i.Id.Value, i.Name, i.Quantity, i.Note, i.DepartmentId.Value, d.Name, d.Icon, d.SortOrder, i.Urgent, i.Status,
            i.AddedBy, i.AddedAtUtc, i.BoughtBy, i.BoughtAtUtc, i.Version);

    /// <summary>The items with these ids, in the order of the ids; ids that no longer exist are left out.</summary>
    public static async Task<IReadOnlyList<ShoppingItemView>> ByIdsAsync(
        IShoppingDbContext db, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return [];

        var itemIds = ids.Select(id => new ShoppingItemId(id)).ToList();
        var rows = await Rows(db, db.Items.Where(i => itemIds.Contains(i.Id))).ToListAsync(cancellationToken);
        var views = rows.Select(ToView).ToDictionary(v => v.Id);
        return ids.Where(views.ContainsKey).Select(id => views[id]).ToList();
    }

    /// <summary>
    /// The store walk: needed items by department order, urgent first, then by when they were added; then the cart,
    /// the latest bought first.
    /// </summary>
    public static IReadOnlyList<ShoppingItemView> InListOrder(IEnumerable<ShoppingItemView> items)
    {
        var all = items.ToList();
        var needed = all
            .Where(v => v.Status == ShoppingItemStatus.Needed)
            .OrderBy(v => v.DepartmentSortOrder)
            .ThenBy(v => v.DepartmentName, StringComparer.Ordinal)
            .ThenByDescending(v => v.Urgent)
            .ThenBy(v => v.AddedAtUtc)
            .ThenBy(v => v.Id);
        var cart = all
            .Where(v => v.Status == ShoppingItemStatus.Bought)
            .OrderByDescending(v => v.BoughtAtUtc)
            .ThenBy(v => v.Id);
        return needed.Concat(cart).ToList();
    }
}

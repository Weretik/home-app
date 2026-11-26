using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>
/// The arithmetic of the summary over what was read: the counts, the departments with needed items in store order,
/// the next items to buy (urgent first, keeping the list order) and the most bought names. Pure, so it is unit-tested
/// with fixed rows.
/// </summary>
internal static class SummaryFigures
{
    public static ShoppingSummary Build(
        IReadOnlyList<ShoppingItemView> list,
        int boughtLast7Days,
        int boughtLast30Days,
        IReadOnlyList<FrequentName> top,
        int nextItemsCount)
    {
        var needed = list.Where(v => v.Status == ShoppingItemStatus.Needed).ToList();
        var departments = needed
            .GroupBy(v => (v.DepartmentId, v.DepartmentName, v.DepartmentIcon, v.DepartmentSortOrder))
            .OrderBy(g => g.Key.DepartmentSortOrder)
            .ThenBy(g => g.Key.DepartmentName, StringComparer.Ordinal)
            .Select(g => new SummaryDepartment(g.Key.DepartmentId, g.Key.DepartmentName, g.Key.DepartmentIcon, g.Count()))
            .ToList();
        var next = needed.OrderByDescending(v => v.Urgent).Take(nextItemsCount).ToList();

        return new ShoppingSummary(
            needed.Count,
            needed.Count(v => v.Urgent),
            list.Count - needed.Count,
            boughtLast7Days,
            boughtLast30Days,
            departments,
            next,
            top.Select(f => new SummaryFrequentItem(f.Name, f.TimesBought)).ToList());
    }
}

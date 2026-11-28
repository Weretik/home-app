using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;

namespace Shopping.Application.Features.Items.Views;

/// <summary>
/// The bot's «⭐ Частые»: bought items of the window grouped by normalised name, the most bought first (then the most
/// recently bought), without names that are in the list now.
/// </summary>
public static class FrequentStatistics
{
    public static async Task<IReadOnlyList<FrequentName>> TopAsync(
        IShoppingDbContext db, ItemWindow window, int count, CancellationToken cancellationToken)
    {
        var since = window.FrequentSince;
        var inList = db.Items.Where(CartWindow.InList(window.Cutoff)).Select(i => i.NormalizedName);
        var bought = db.Items.AsNoTracking()
            .Where(i => i.Status == ShoppingItemStatus.Bought && i.BoughtAtUtc >= since)
            .Where(i => !inList.Contains(i.NormalizedName));

        var top = await bought
            .GroupBy(i => i.NormalizedName)
            .Select(g => new { Name = g.Key, Times = g.Count(), Last = g.Max(i => i.BoughtAtUtc) })
            .OrderByDescending(g => g.Times)
            .ThenByDescending(g => g.Last)
            .ThenBy(g => g.Name)
            .Take(count)
            .ToListAsync(cancellationToken);
        if (top.Count == 0)
            return [];

        // The latest spelling and department of each top name; bounded by the window.
        var names = top.Select(t => t.Name).ToList();
        var latest = (await bought
                .Where(i => names.Contains(i.NormalizedName))
                .Select(i => new { i.NormalizedName, i.Name, i.DepartmentId, i.BoughtAtUtc })
                .ToListAsync(cancellationToken))
            .GroupBy(i => i.NormalizedName)
            .ToDictionary(g => g.Key, g => g.MaxBy(i => i.BoughtAtUtc)!);

        return top
            .Select(t => new FrequentName(latest[t.Name].Name, latest[t.Name].DepartmentId.Value, t.Times, t.Last!.Value))
            .ToList();
    }
}

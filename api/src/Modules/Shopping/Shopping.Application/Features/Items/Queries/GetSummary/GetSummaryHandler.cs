using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>Computed on read from the list, two counts of bought rows and the frequent statistics; nothing is stored.</summary>
public sealed class GetSummaryHandler(IShoppingDbContext db, TimeProvider clock) : IQueryHandler<GetSummaryQuery, Result<ShoppingSummary>>
{
    public const int NextItemsCount = 8;
    public const int TopFrequentCount = 5;

    public async ValueTask<Result<ShoppingSummary>> Handle(GetSummaryQuery query, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var rows = await ItemViews.Rows(db, db.Items.Where(CartWindow.InList(window.Cutoff))).ToListAsync(cancellationToken);
        var list = ItemViews.InListOrder(rows.Select(ItemViews.ToView));
        var needed = list.Where(v => v.Status == ShoppingItemStatus.Needed).ToList();

        var weekAgo = window.Now.AddDays(-7);
        var monthAgo = window.Now.AddDays(-30);
        var boughtLast7Days = await db.Items.CountAsync(i => i.Status == ShoppingItemStatus.Bought && i.BoughtAtUtc >= weekAgo, cancellationToken);
        var boughtLast30Days = await db.Items.CountAsync(i => i.Status == ShoppingItemStatus.Bought && i.BoughtAtUtc >= monthAgo, cancellationToken);
        var top = await FrequentStatistics.TopAsync(db, window, TopFrequentCount, cancellationToken);

        var departments = needed
            .GroupBy(v => (v.DepartmentId, v.DepartmentName, v.DepartmentIcon, v.DepartmentSortOrder))
            .OrderBy(g => g.Key.DepartmentSortOrder)
            .ThenBy(g => g.Key.DepartmentName, StringComparer.Ordinal)
            .Select(g => new SummaryDepartment(g.Key.DepartmentId, g.Key.DepartmentName, g.Key.DepartmentIcon, g.Count()))
            .ToList();
        var next = needed.OrderByDescending(v => v.Urgent).Take(NextItemsCount).ToList();

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

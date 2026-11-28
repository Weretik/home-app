using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;

namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>
/// Computed on read from the list, two counts of bought rows and the frequent statistics (put together by
/// <see cref="SummaryFigures"/>); nothing is stored.
/// </summary>
public sealed class GetSummaryHandler(IShoppingDbContext db, TimeProvider clock) : IQueryHandler<GetSummaryQuery, Result<ShoppingSummary>>
{
    public const int NextItemsCount = 8;
    public const int TopFrequentCount = 5;

    public async ValueTask<Result<ShoppingSummary>> Handle(GetSummaryQuery query, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var rows = await ItemViews.Rows(db, db.Items.AsNoTracking().Where(CartWindow.InList(window.Cutoff))).ToListAsync(cancellationToken);

        var weekAgo = window.Now.AddDays(-7);
        var monthAgo = window.Now.AddDays(-30);
        var boughtLast7Days = await db.Items.AsNoTracking().CountAsync(i => i.Status == ShoppingItemStatus.Bought && i.BoughtAtUtc >= weekAgo, cancellationToken);
        var boughtLast30Days = await db.Items.AsNoTracking().CountAsync(i => i.Status == ShoppingItemStatus.Bought && i.BoughtAtUtc >= monthAgo, cancellationToken);
        var top = await FrequentStatistics.TopAsync(db, window, TopFrequentCount, cancellationToken);

        return SummaryFigures.Build(
            ItemViews.InListOrder(rows.Select(ItemViews.ToView)), boughtLast7Days, boughtLast30Days, top, NextItemsCount);
    }
}

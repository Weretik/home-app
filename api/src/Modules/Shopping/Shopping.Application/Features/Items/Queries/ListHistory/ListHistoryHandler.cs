using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Queries.ListHistory;

public sealed class ListHistoryHandler(IShoppingDbContext db, TimeProvider clock)
    : IQueryHandler<ListHistoryQuery, Result<ShoppingHistoryPage>>
{
    public async ValueTask<Result<ShoppingHistoryPage>> Handle(ListHistoryQuery query, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var page = await ItemViews.Rows(db, db.Items.AsNoTracking().Where(CartWindow.InHistory(window.Cutoff)))
            .OrderByDescending(r => r.Item.BoughtAtUtc)
            .ThenByDescending(r => r.Item.Id)
            .ToPagedListAsync(query.Page, query.PageSize, cancellationToken);

        return new ShoppingHistoryPage(page.Items.Select(ItemViews.ToView).ToList(), page.Page, page.PageSize, page.TotalCount);
    }
}

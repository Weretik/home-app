using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Queries.ListItems;

/// <summary>The list is small by nature (what is needed now plus a few hours of the cart), so it is read whole.</summary>
public sealed class ListItemsHandler(IShoppingDbContext db, TimeProvider clock) : IQueryHandler<ListItemsQuery, Result<ShoppingList>>
{
    public async ValueTask<Result<ShoppingList>> Handle(ListItemsQuery query, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var rows = await ItemViews.Rows(db, db.Items.Where(CartWindow.InList(window.Cutoff))).ToListAsync(cancellationToken);
        return new ShoppingList(ItemViews.InListOrder(rows.Select(ItemViews.ToView)));
    }
}

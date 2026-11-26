using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.SetItemStatus;

public sealed class SetItemStatusHandler(IShoppingDbContext db, TimeProvider clock)
    : ICommandHandler<SetItemStatusCommand, Result<ShoppingItemView>>
{
    public async ValueTask<Result<ShoppingItemView>> Handle(SetItemStatusCommand c, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var item = await db.Items.FirstOrDefaultAsync(i => i.Id == c.Id, cancellationToken);
        // History no longer changes: «вернуть в список» is an add.
        if (item is null || !CartWindow.IsInList(item, window.Cutoff))
            return ShoppingItemErrors.NotFound(c.Id);

        item.SetStatus(c.Status, c.By, window.Now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        var department = await db.Departments.AsNoTracking().FirstAsync(d => d.Id == item.DepartmentId, cancellationToken);
        return ItemViews.ToView(item, department);
    }
}

using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.DeleteItem;

public sealed class DeleteItemHandler(IShoppingDbContext db) : ICommandHandler<DeleteItemCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteItemCommand c, CancellationToken cancellationToken)
    {
        var item = await db.Items.FirstOrDefaultAsync(i => i.Id == c.Id, cancellationToken);
        if (item is null)
            return ShoppingItemErrors.NotFound(c.Id);
        if (item.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;

        db.Items.Remove(item);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

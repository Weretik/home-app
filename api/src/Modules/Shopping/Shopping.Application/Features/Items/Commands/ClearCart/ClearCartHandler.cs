using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.ClearCart;

/// <summary>One conditional <c>UPDATE</c>: only rows still in the cart are cleared, so two presses do not race.</summary>
public sealed class ClearCartHandler(IShoppingDbContext db, TimeProvider clock)
    : ICommandHandler<ClearCartCommand, Result<ClearCartResult>>
{
    public async ValueTask<Result<ClearCartResult>> Handle(ClearCartCommand c, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var now = window.Now;
        var cleared = await db.Items
            .Where(CartWindow.InCart(window.Cutoff))
            .ExecuteUpdateAsync(set => set.SetProperty(i => i.ClearedAtUtc, (DateTimeOffset?)now), cancellationToken);
        return new ClearCartResult(cleared);
    }
}

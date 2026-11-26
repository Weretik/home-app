using BuildingBlocks.Application.Outbox;
using Gifts.Contracts;
using Microsoft.EntityFrameworkCore;
using Todos.Contracts;
using Todos.Domain.Tasks;

namespace Todos.Application.Integrations.Gifts;

/// <summary>
/// The gift is no longer planned (spec gifts/002, R-005): gifted → the task is done, anything else → cancelled. An
/// archived or missing task is left alone, and nothing is published back. Safe to repeat.
/// </summary>
public sealed class GiftPurchaseClosedHandler(ITodosDbContext db, TimeProvider clock) : IOutboxMessageHandler<GiftPurchaseClosed>
{
    public async Task HandleAsync(GiftPurchaseClosed message, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(
            t => t.SourceKind == TodoTaskCompleted.GiftSource && t.SourceId == message.GiftId, cancellationToken);
        if (task is null || task.IsArchived)
            return;

        task.ChangeStatus(message.Outcome == GiftPurchaseOutcome.Gifted ? TodoStatus.Done : TodoStatus.Cancelled, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }
}

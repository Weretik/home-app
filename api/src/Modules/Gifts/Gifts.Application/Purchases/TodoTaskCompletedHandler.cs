using BuildingBlocks.Application.Outbox;
using Gifts.Domain.Wishlist;
using Gifts.Domain.Wishlist.Identifiers;
using Microsoft.EntityFrameworkCore;
using Todos.Contracts;

namespace Gifts.Application.Purchases;

/// <summary>
/// A to-do that came from a gift was ticked done: the gift moves to «Подаренные» with today's date (spec gifts/002,
/// R-006). Safe to repeat: a gift that is already gifted, dropped or gone is left alone, and nothing is published
/// back, so the task is not touched again.
/// </summary>
public sealed class TodoTaskCompletedHandler(IGiftsDbContext db, TimeProvider clock) : IOutboxMessageHandler<TodoTaskCompleted>
{
    public async Task HandleAsync(TodoTaskCompleted message, CancellationToken cancellationToken)
    {
        if (message.SourceKind != TodoTaskCompleted.GiftSource)
            return;

        var gift = await db.Gifts.FirstOrDefaultAsync(g => g.Id == new GiftId(message.SourceId), cancellationToken);
        if (gift is null || gift.Status != GiftStatus.Wanted)
            return;

        // Today is always in range, so the status change cannot fail.
        gift.ChangeStatus(GiftStatus.Gifted, null, HouseholdDay.Today(clock));
        await db.SaveChangesAsync(cancellationToken);
    }
}

using BuildingBlocks.Application.Outbox;
using Gifts.Contracts;
using Microsoft.EntityFrameworkCore;
using Todos.Contracts;
using Todos.Domain.Tasks;

namespace Todos.Application.Integrations.Gifts;

/// <summary>
/// Keeps the giver's to-do of a planned gift (spec gifts/002, R-004): creates it the first time, then follows the
/// gift's name, giver and date, and reopens it when it was archived. Safe to repeat: one task per gift (unique
/// source index), and a second run writes the same values. A lost race throws, so the outbox tries again.
/// </summary>
public sealed class GiftPurchasePlannedHandler(ITodosDbContext db, TimeProvider clock) : IOutboxMessageHandler<GiftPurchasePlanned>
{
    public async Task HandleAsync(GiftPurchasePlanned message, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var title = GiftTask.Title(message.GiftName, message.Recipient);
        var assignee = GiftTask.AssigneeOf(message.Giver);
        var task = await db.Tasks.FirstOrDefaultAsync(
            t => t.SourceKind == TodoTaskCompleted.GiftSource && t.SourceId == message.GiftId, cancellationToken);

        if (task is null)
        {
            var created = TodoTask.Create(
                title, GiftTask.Notes(message.Link), assignee, null, TodoStatus.Todo, TodoPriority.High, message.BuyBy,
                await ShoppingCategoryAsync(cancellationToken), [], now);
            if (created.IsFailure || created.Value.LinkSource(TodoTaskCompleted.GiftSource, message.GiftId).IsFailure)
                return; // Cannot happen: the title is cut to fit and the date comes from a valid gift.
            db.Tasks.Add(created.Value);
        }
        else
        {
            // The person's notes, author, priority and category stay; an archived task comes back.
            var status = task.IsArchived ? TodoStatus.Todo : task.Status;
            if (task.Update(title, task.Notes, assignee, task.Author, status, task.Priority, message.BuyBy, task.CategoryId, now).IsFailure)
                return;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid?> ShoppingCategoryAsync(CancellationToken cancellationToken) =>
        await db.Categories.AnyAsync(c => c.Id == GiftTask.ShoppingCategoryId, cancellationToken) ? GiftTask.ShoppingCategoryId : null;
}

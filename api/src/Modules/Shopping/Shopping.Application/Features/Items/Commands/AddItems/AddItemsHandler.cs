using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Dictionary;
using Shopping.Application.Features.Items.Views;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;
using Shopping.Domain.Parsing;

namespace Shopping.Application.Features.Items.Commands.AddItems;

/// <summary>
/// Reads the text, then for each item: the same name needed → refresh it; the same name in the cart → it is needed
/// again; otherwise a new row in the matched (or chosen) department. One save, with the Idempotency-Key.
/// </summary>
public sealed class AddItemsHandler(IShoppingDbContext db, TimeProvider clock)
    : ICommandHandler<AddItemsCommand, Result<AddItemsResult>>
{
    private const string Kind = "items";

    public async ValueTask<Result<AddItemsResult>> Handle(AddItemsCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Text, c.AddedBy, c.DepartmentId, c.Urgent });
        if (await IdempotentCreates.EarlierOutcomeAsync<AddItemsOutcome>(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier.IsFailure ? earlier.Error! : await ResultAsync(earlier.Value, cancellationToken);

        var parsed = ItemTextParser.Parse(c.Text);
        if (parsed.Count == 0)
            return ShoppingItemErrors.NothingToAdd;
        if (parsed.Count > ShoppingItem.MaxItemsPerAdd)
            return ShoppingItemErrors.TooManyItems;

        var chosen = c.DepartmentId is { } value ? new DepartmentId(value) : (DepartmentId?)null;
        if (chosen is { } department && !await db.Departments.AnyAsync(d => d.Id == department, cancellationToken))
            return DepartmentErrors.NotFound(department);

        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var names = parsed.Select(p => NameNormalizer.Normalize(p.Name)).Distinct().ToList();
        var inList = await db.Items
            .Where(CartWindow.InList(window.Cutoff))
            .Where(i => names.Contains(i.NormalizedName))
            .ToListAsync(cancellationToken);

        var dictionary = c.DepartmentId is null ? await DepartmentDictionary.LoadAsync(db, cancellationToken) : null;
        var added = new List<Guid>();
        var restored = new List<Guid>();
        var updated = new List<Guid>();
        foreach (var item in parsed)
        {
            var normalized = NameNormalizer.Normalize(item.Name);
            var same = inList
                .Where(i => i.NormalizedName == normalized)
                .OrderBy(i => i.Status == ShoppingItemStatus.Needed ? 0 : 1)
                .ThenByDescending(i => i.BoughtAtUtc)
                .FirstOrDefault();

            if (same is not null)
            {
                if (same.AddAgain(item.Quantity, c.Urgent, c.AddedBy, window.Now) == ReAddOutcome.Updated)
                    AddOnce(updated, same.Id.Value, added, restored);
                else
                    AddOnce(restored, same.Id.Value, added, updated);
            }
            else
            {
                var departmentId = chosen ?? dictionary!.Matcher.Match(item.Name);
                var created = ShoppingItem.Create(item.Name, item.Quantity, null, departmentId, c.Urgent, c.AddedBy, window.Now);
                if (created.IsFailure)
                    return created.Error!;
                db.Items.Add(created.Value);
                inList.Add(created.Value);
                added.Add(created.Value.Id.Value);
            }
        }

        var outcome = new AddItemsOutcome(added, restored, updated);
        db.CreateRequests.Add(IdempotentCreates.Record(c.IdempotencyKey, Kind, fingerprint, outcome, window.Now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of the same request saved first: answer like a repeat. Without a winner the clash was
            // another row's unique key, a race like any other: 409, never 500.
            var winner = await IdempotentCreates.EarlierOutcomeAsync<AddItemsOutcome>(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken);
            if (winner is null)
                return ShoppingItemErrors.ChangedMeanwhile;
            return winner.IsFailure ? winner.Error! : await ResultAsync(winner.Value, cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        return await ResultAsync(outcome, cancellationToken);
    }

    /// <summary>An item already reported in another group of this add (added, then repeated) stays where it is.</summary>
    private static void AddOnce(List<Guid> target, Guid id, List<Guid> other1, List<Guid> other2)
    {
        if (!target.Contains(id) && !other1.Contains(id) && !other2.Contains(id))
            target.Add(id);
    }

    private async Task<AddItemsResult> ResultAsync(AddItemsOutcome outcome, CancellationToken cancellationToken)
    {
        var views = await ItemViews.ByIdsAsync(db, [.. outcome.Added, .. outcome.Restored, .. outcome.Updated], cancellationToken);
        IReadOnlyList<ShoppingItemView> Of(IReadOnlyList<Guid> ids) => views.Where(v => ids.Contains(v.Id)).ToList();
        return new AddItemsResult(Of(outcome.Added), Of(outcome.Restored), Of(outcome.Updated));
    }
}

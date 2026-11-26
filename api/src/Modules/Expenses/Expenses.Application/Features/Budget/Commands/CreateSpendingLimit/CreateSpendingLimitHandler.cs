using Expenses.Application;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Budget;
using Expenses.Domain.Categories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;

/// <summary>
/// Adds a limit at the end of the list (<c>max + 10</c>). The read of the max and the insert race when two limits are
/// added at once; the unique index <see cref="SortOrderIndex"/> stops the second one, which answers 409.
/// </summary>
public sealed class CreateSpendingLimitHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<CreateSpendingLimitCommand, Result<CreatedEntry>>
{
    /// <summary>The unique index on <c>spending_limits.sort_order</c>, the race guard of the list order.</summary>
    public const string SortOrderIndex = "ux_spending_limits_sort_order";

    private const string Kind = "limit";

    public async ValueTask<Result<CreatedEntry>> Handle(CreateSpendingLimitCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Name, c.Period, c.Amount, c.CategoryIds });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (await BudgetCategories.FirstMissingAsync(db, c.CategoryIds, cancellationToken) is { } missing)
            return CategoryErrors.NotFoundById(missing);

        var last = await db.SpendingLimits.MaxAsync(l => (int?)l.SortOrder, cancellationToken) ?? 0;
        var now = clock.GetUtcNow();
        var created = SpendingLimit.Create(c.Name, c.Period, c.Amount, c.CategoryIds, last + 10, now);
        if (created.IsFailure)
            return created.Error!;

        db.SpendingLimits.Add(created.Value);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException violation) when (violation.ConstraintName == SortOrderIndex)
        {
            return BudgetErrors.LimitAddedMeanwhile;
        }
        catch (UniqueConstraintViolationException)
        {
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? IdempotentCreates.KeyReused;
        }

        return new CreatedEntry(created.Value.Id);
    }
}

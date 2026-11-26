using Expenses.Application;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Budget;
using Expenses.Domain.Categories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;

public sealed class CreateSpendingLimitHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<CreateSpendingLimitCommand, Result<CreatedEntry>>
{
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
        catch (UniqueConstraintViolationException)
        {
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? IdempotentCreates.KeyReused;
        }

        return new CreatedEntry(created.Value.Id);
    }
}

using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Categories;
using Expenses.Domain.Planning;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Planned.Commands.CreatePlannedExpense;

public sealed class CreatePlannedExpenseHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<CreatePlannedExpenseCommand, Result<CreatedEntry>>
{
    private const string Kind = "planned";

    public async ValueTask<Result<CreatedEntry>> Handle(CreatePlannedExpenseCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Year, c.Month, c.Name, c.CategoryId, c.Payer, c.Amount });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (!await db.Categories.AnyAsync(x => x.Id == new CategoryId(c.CategoryId), cancellationToken))
            return CategoryErrors.NotFoundById(new CategoryId(c.CategoryId));

        var now = clock.GetUtcNow();
        var created = PlannedExpense.Create(new DateOnly(c.Year, c.Month, 1), c.Name, new CategoryId(c.CategoryId), c.Payer, c.Amount, now);
        if (created.IsFailure)
            return created.Error!;

        db.PlannedExpenses.Add(created.Value);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id.Value, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of the same request saved first: answer like a repeat.
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? IdempotentCreates.KeyReused;
        }

        return new CreatedEntry(created.Value.Id.Value);
    }
}

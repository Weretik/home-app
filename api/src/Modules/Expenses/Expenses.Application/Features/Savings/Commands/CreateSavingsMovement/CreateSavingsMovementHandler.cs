using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Savings;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Savings.Commands.CreateSavingsMovement;

public sealed class CreateSavingsMovementHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<CreateSavingsMovementCommand, Result<CreatedEntry>>
{
    private const string Kind = "savings";

    public async ValueTask<Result<CreatedEntry>> Handle(CreateSavingsMovementCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Date, c.Person, c.Amount, c.Comment });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        var now = clock.GetUtcNow();
        var created = SavingsMovement.Create(c.Date, c.Person, c.Amount, c.Comment, now);
        if (created.IsFailure)
            return created.Error!;

        db.SavingsMovements.Add(created.Value);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id, now));
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

        return new CreatedEntry(created.Value.Id);
    }
}

using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Expenses.Commands.CreateExpense;

public sealed class CreateExpenseHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<CreateExpenseCommand, Result<CreatedEntry>>
{
    private const string Kind = "expense";

    public async ValueTask<Result<CreatedEntry>> Handle(CreateExpenseCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Date, c.Payer, c.CategoryId, c.Amount, c.PaymentMethod, c.Comment });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (!await db.Categories.AnyAsync(x => x.Id == c.CategoryId, cancellationToken))
            return CategoryErrors.NotFoundById(c.CategoryId);

        var now = clock.GetUtcNow();
        var created = Expense.Create(c.Date, c.Payer, c.CategoryId, c.Amount, c.PaymentMethod, c.Comment, now);
        if (created.IsFailure)
            return created.Error!;

        db.Expenses.Add(created.Value);
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

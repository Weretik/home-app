using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Keywords.Commands.CreateKeyword;

public sealed class CreateKeywordHandler(IShoppingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateKeywordCommand, Result<ShoppingCreatedId>>
{
    private const string Kind = "keyword";

    public async ValueTask<Result<ShoppingCreatedId>> Handle(CreateKeywordCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Keyword, c.DepartmentId });
        if (await IdempotentCreates.EarlierOutcomeAsync<ShoppingCreatedId>(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (!await db.Departments.AnyAsync(d => d.Id == c.DepartmentId, cancellationToken))
            return DepartmentErrors.NotFound(c.DepartmentId);

        var now = clock.GetUtcNow();
        var created = DepartmentKeyword.Create(c.Keyword, c.DepartmentId, learned: false, now);
        if (created.IsFailure)
            return created.Error!;
        var keyword = created.Value.Keyword;
        if (await db.Keywords.AnyAsync(k => k.Keyword == keyword, cancellationToken))
            return DepartmentErrors.KeywordTaken;

        db.Keywords.Add(created.Value);
        var outcome = new ShoppingCreatedId(created.Value.Id);
        db.CreateRequests.Add(IdempotentCreates.Record(c.IdempotencyKey, Kind, fingerprint, outcome, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of this request (the key), or the same keyword added by someone else.
            var winner = await IdempotentCreates.EarlierOutcomeAsync<ShoppingCreatedId>(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken);
            return winner ?? Result<ShoppingCreatedId>.Failure(DepartmentErrors.KeywordTaken);
        }

        return outcome;
    }
}

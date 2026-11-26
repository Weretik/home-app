using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Gifts.Application.Idempotency;
using Gifts.Domain.People;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Details.Commands.CreateDetail;

public sealed class CreateDetailHandler(IGiftsDbContext db, TimeProvider clock)
    : ICommandHandler<CreateDetailCommand, Result<CreatedGiftsEntry>>
{
    private const string Kind = "detail";

    public async ValueTask<Result<CreatedGiftsEntry>> Handle(CreateDetailCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Recipient, c.Label, c.Value });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        var last = await db.Details.Where(d => d.Recipient == c.Recipient).MaxAsync(d => (int?)d.SortOrder, cancellationToken) ?? 0;
        var now = clock.GetUtcNow();
        var created = Detail.Create(c.Recipient, c.Label, c.Value, last + 10, now);
        if (created.IsFailure)
            return created.Error!;

        db.Details.Add(created.Value);
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

        return new CreatedGiftsEntry(created.Value.Id);
    }
}

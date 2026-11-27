using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Gifts.Application.Features.Categories;
using Gifts.Application.Idempotency;
using Gifts.Domain.Categories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryHandler(IGiftsDbContext db, TimeProvider clock)
    : ICommandHandler<CreateCategoryCommand, Result<CreatedGiftsEntry>>
{
    private const string Kind = "category";

    public async ValueTask<Result<CreatedGiftsEntry>> Handle(CreateCategoryCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Name });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        var last = await db.Categories.MaxAsync(x => (int?)x.SortOrder, cancellationToken) ?? 0;
        var now = clock.GetUtcNow();
        var created = Category.Create(c.Name, last + 10, now);
        if (created.IsFailure)
            return created.Error!;

        db.Categories.Add(created.Value);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id.Value, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException exception)
        {
            // The key won by a racing copy is a repeat; a taken name is the family's answer to fix.
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? (CategoryNames.IsNameIndex(exception) ? CategoryErrors.NameTaken : IdempotentCreates.KeyReused);
        }

        return new CreatedGiftsEntry(created.Value.Id.Value);
    }
}

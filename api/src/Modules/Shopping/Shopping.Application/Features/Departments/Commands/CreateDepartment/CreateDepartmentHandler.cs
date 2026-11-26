using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Departments.Commands.CreateDepartment;

/// <summary>The new department takes the fallback's place; the fallback and everything after it move one step on.</summary>
public sealed class CreateDepartmentHandler(IShoppingDbContext db, TimeProvider clock)
    : ICommandHandler<CreateDepartmentCommand, Result<ShoppingCreatedId>>
{
    private const string Kind = "department";

    public async ValueTask<Result<ShoppingCreatedId>> Handle(CreateDepartmentCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[] { c.Name, c.Icon });
        if (await IdempotentCreates.EarlierOutcomeAsync<ShoppingCreatedId>(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        var name = c.Name.Trim();
        if (await db.Departments.AnyAsync(d => d.Name == name, cancellationToken))
            return DepartmentErrors.NameTaken;

        var departments = await db.Departments.ToListAsync(cancellationToken);
        var fallback = departments.Single(d => d.IsFallback);
        var place = fallback.SortOrder;
        var now = clock.GetUtcNow();
        var created = Department.Create(name, c.Icon, place, now);
        if (created.IsFailure)
            return created.Error!;

        foreach (var later in departments.Where(d => d.SortOrder >= place))
            later.MoveTo(later.SortOrder + Department.SortStep);
        db.Departments.Add(created.Value);
        var outcome = new ShoppingCreatedId(created.Value.Id);
        db.CreateRequests.Add(IdempotentCreates.Record(c.IdempotencyKey, Kind, fingerprint, outcome, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of this request (the key), or another department with this name.
            var winner = await IdempotentCreates.EarlierOutcomeAsync<ShoppingCreatedId>(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken);
            return winner ?? Result<ShoppingCreatedId>.Failure(DepartmentErrors.NameTaken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        return outcome;
    }
}

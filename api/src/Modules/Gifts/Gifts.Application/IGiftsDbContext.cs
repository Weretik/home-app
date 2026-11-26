using Gifts.Application.Idempotency;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application;

/// <summary>
/// The module's unit of work: handlers query the sets and call <see cref="SaveChangesAsync"/> once. Infrastructure
/// implements it with the EF Core context that lives in the <c>gifts</c> schema. Variants have no set of their own:
/// they load and save with their gift.
/// </summary>
public interface IGiftsDbContext
{
    DbSet<Gift> Gifts { get; }

    DbSet<Category> Categories { get; }

    DbSet<Detail> Details { get; }

    DbSet<CreateRequest> CreateRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

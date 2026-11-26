using Gifts.Application;
using Gifts.Application.Idempotency;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.TestHost;

/// <summary>
/// The Gifts unit of work with a race built in: <paramref name="meanwhile"/> runs once, right before the save, after
/// the handler has read and checked its rows (the same window as <see cref="RacingExpensesDb"/>), so the guards
/// (row version, foreign key) can be tested deterministically on the real database.
/// </summary>
internal sealed class RacingGiftsDb(IGiftsDbContext inner, Func<Task> meanwhile) : IGiftsDbContext
{
    private bool _raced;

    public DbSet<Gift> Gifts => inner.Gifts;

    public DbSet<Category> Categories => inner.Categories;

    public DbSet<Detail> Details => inner.Details;

    public DbSet<CreateRequest> CreateRequests => inner.CreateRequests;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_raced)
        {
            _raced = true;
            await meanwhile();
        }

        return await inner.SaveChangesAsync(cancellationToken);
    }
}

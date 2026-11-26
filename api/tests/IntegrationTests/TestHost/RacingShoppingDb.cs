using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shopping.Application;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Settings;

namespace IntegrationTests.TestHost;

/// <summary>
/// The Shopping unit of work with a race built in: <paramref name="meanwhile"/> runs once, right before the save,
/// after the handler has read, checked and (for a department delete) moved its rows, so the guards (row version,
/// foreign key) can be tested deterministically on the real database.
/// </summary>
internal sealed class RacingShoppingDb(IShoppingDbContext inner, Func<Task> meanwhile) : IShoppingDbContext
{
    private bool _raced;

    public DbSet<ShoppingItem> Items => inner.Items;

    public DbSet<Department> Departments => inner.Departments;

    public DbSet<DepartmentKeyword> Keywords => inner.Keywords;

    public DbSet<ShoppingSettings> Settings => inner.Settings;

    public DbSet<CreateRequest> CreateRequests => inner.CreateRequests;

    public DatabaseFacade Database => inner.Database;

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

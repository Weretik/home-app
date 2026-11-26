using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Shopping.Infrastructure;

/// <summary>Local development and integration tests: applies the module's migrations (the dictionary comes with them).</summary>
internal sealed class ShoppingDatabaseInitializer(ShoppingDbContext db) : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}

using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Infrastructure;

/// <summary>Local development and integration tests: applies the module's migrations (categories come with them).</summary>
internal sealed class ExpensesDatabaseInitializer(ExpensesDbContext db) : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}

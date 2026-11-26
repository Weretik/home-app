using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Infrastructure;

/// <summary>Local development and integration tests: applies the module's migrations.</summary>
internal sealed class NotificationsDatabaseInitializer(NotificationsDbContext db) : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken) => db.Database.MigrateAsync(cancellationToken);
}

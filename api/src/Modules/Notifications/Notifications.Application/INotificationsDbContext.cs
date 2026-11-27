using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Push;
using Notifications.Domain.Schedule;

namespace Notifications.Application;

/// <summary>
/// The module's unit of work: handlers query the sets and call <see cref="SaveChangesAsync"/> once. A planned message
/// is a <see cref="Delivery"/> plus an outbox message (<see cref="IOutboxPublisher.Publish"/>) in the same save.
/// </summary>
public interface INotificationsDbContext : IOutboxPublisher
{
    DbSet<LinkedChat> Chats { get; }

    DbSet<Delivery> Deliveries { get; }

    DbSet<PushSubscription> PushSubscriptions { get; }

    DbSet<SchedulerHeartbeat> Heartbeats { get; }

    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Notifications.Application;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Push;
using Notifications.Domain.Schedule;

namespace Notifications.Infrastructure;

/// <summary>The module's tables, all in the <c>notifications</c> schema, with their own migration history and outbox.</summary>
public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : ModuleDbContext(options, Schema), INotificationsDbContext
{
    public const string Schema = "notifications";

    public DbSet<LinkedChat> Chats => Set<LinkedChat>();

    public DbSet<Delivery> Deliveries => Set<Delivery>();

    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    public DbSet<SchedulerHeartbeat> Heartbeats => Set<SchedulerHeartbeat>();

    /// <summary>Planned messages are sent by the outbox (ADR 0033).</summary>
    protected override bool HasOutbox => true;
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Schedule;

namespace Notifications.Infrastructure.Persistence;

/// <summary><c>notifications.scheduler_heartbeat</c>: exactly one row, seeded by the migration, updated by every tick.</summary>
internal sealed class SchedulerHeartbeatConfiguration : IEntityTypeConfiguration<SchedulerHeartbeat>
{
    public void Configure(EntityTypeBuilder<SchedulerHeartbeat> builder)
    {
        builder.ToTable("scheduler_heartbeat");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(h => h.LastTickAtUtc).HasColumnName("last_tick_at_utc");
        builder.HasData(new { Id = SchedulerHeartbeat.SingletonId, LastTickAtUtc = (DateTimeOffset?)null });
    }
}

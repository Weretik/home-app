using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Push;

namespace Notifications.Infrastructure.Persistence;

/// <summary><c>notifications.push_subscriptions</c>: the unique endpoint makes saving an upsert (spec notifications/002, R-102).</summary>
internal sealed class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("push_subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.Person).HasColumnName("person").HasConversion<string>().HasMaxLength(16);
        builder.Property(s => s.Endpoint).HasColumnName("endpoint").HasMaxLength(PushSubscription.MaxEndpointLength).IsRequired();
        builder.Property(s => s.P256dh).HasColumnName("p256dh").HasMaxLength(PushSubscription.MaxP256dhLength).IsRequired();
        builder.Property(s => s.Auth).HasColumnName("auth").HasMaxLength(PushSubscription.MaxAuthLength).IsRequired();
        builder.Property(s => s.Device).HasColumnName("device").HasMaxLength(PushSubscription.MaxDeviceLength);
        builder.Property(s => s.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(s => s.LastSeenAtUtc).HasColumnName("last_seen_at_utc");
        builder.HasIndex(s => s.Endpoint).IsUnique().HasDatabaseName("ux_push_subscriptions_endpoint");
        builder.HasIndex(s => s.Person).HasDatabaseName("ix_push_subscriptions_person");
    }
}

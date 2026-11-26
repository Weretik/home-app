using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Deliveries;

namespace Notifications.Infrastructure.Persistence;

/// <summary><c>notifications.deliveries</c>: the unique key makes planning idempotent (R-007).</summary>
internal sealed class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("deliveries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.Key).HasColumnName("key").HasMaxLength(Delivery.MaxKeyLength).IsRequired();
        builder.Property(d => d.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(32);
        builder.Property(d => d.Audience).HasColumnName("audience").HasConversion<string>().HasMaxLength(16);
        builder.Property(d => d.ChatId).HasColumnName("chat_id");
        builder.Property(d => d.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(d => d.SentAtUtc).HasColumnName("sent_at_utc");
        builder.HasIndex(d => d.Key).IsUnique().HasDatabaseName("ux_deliveries_key");
        builder.HasIndex(d => d.CreatedAtUtc).IsDescending().HasDatabaseName("ix_deliveries_created_at_utc");
    }
}

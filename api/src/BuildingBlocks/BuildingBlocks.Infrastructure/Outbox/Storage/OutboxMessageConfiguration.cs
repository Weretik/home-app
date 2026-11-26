using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Outbox.Storage;

/// <summary>The outbox table of a module; it lives in that module's schema.</summary>
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.LastError).HasMaxLength(OutboxMessage.MaxErrorLength);
        builder.HasIndex(m => new { m.ProcessedAtUtc, m.FailedAtUtc, m.NextAttemptAtUtc });
        builder.HasIndex(m => m.ClaimToken);
    }
}

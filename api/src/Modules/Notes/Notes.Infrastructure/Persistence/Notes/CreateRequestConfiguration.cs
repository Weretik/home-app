using Notes.Application.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notes.Infrastructure.Persistence.Notes;

internal sealed class CreateRequestConfiguration : IEntityTypeConfiguration<CreateRequest>
{
    public void Configure(EntityTypeBuilder<CreateRequest> builder)
    {
        builder.ToTable("create_requests");
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key).HasColumnName("key").HasMaxLength(CreateRequest.MaxKeyLength);
        builder.Property(r => r.Kind).HasColumnName("kind").HasMaxLength(20);
        builder.Property(r => r.Fingerprint).HasColumnName("fingerprint").HasMaxLength(64);
        builder.Property(r => r.EntityId).HasColumnName("entity_id");
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc");
    }
}

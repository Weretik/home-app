using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopping.Application.Idempotency;

namespace Shopping.Infrastructure.Persistence.Items;

internal sealed class CreateRequestConfiguration : IEntityTypeConfiguration<CreateRequest>
{
    public void Configure(EntityTypeBuilder<CreateRequest> builder)
    {
        builder.ToTable("create_requests");
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key).HasColumnName("key").HasMaxLength(CreateRequest.MaxKeyLength);
        builder.Property(r => r.Kind).HasColumnName("kind").HasMaxLength(20);
        builder.Property(r => r.Fingerprint).HasColumnName("fingerprint").HasMaxLength(64);
        builder.Property(r => r.Outcome).HasColumnName("outcome").IsRequired();
        builder.Property(r => r.CreatedAtUtc).HasColumnName("created_at_utc");
    }
}

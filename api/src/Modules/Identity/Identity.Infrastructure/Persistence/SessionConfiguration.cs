using Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence;

/// <summary><c>identity.sessions</c>: one row per signed-in device, found by the hash of its token.</summary>
internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");
        builder.HasKey(s => s.Id);
        builder.Ignore(s => s.Member);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.TokenHash).HasColumnName("token_hash").HasMaxLength(Session.TokenHashLength).IsFixedLength();
        builder.Property(s => s.Email).HasColumnName("email").HasMaxLength(Session.MaxEmailLength);
        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(Session.MaxNameLength);
        builder.Property(s => s.PictureUrl).HasColumnName("picture_url").HasMaxLength(Session.MaxPictureUrlLength);
        builder.Property(s => s.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(s => s.LastSeenAtUtc).HasColumnName("last_seen_at_utc");
        builder.Property(s => s.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(s => s.RevokedAtUtc).HasColumnName("revoked_at_utc");
        builder.HasIndex(s => s.TokenHash).IsUnique().HasDatabaseName("ux_sessions_token_hash");
        builder.HasIndex(s => s.Email).HasDatabaseName("ix_sessions_email");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Chats;

namespace Notifications.Infrastructure.Persistence;

/// <summary><c>notifications.linked_chats</c>: one chat per audience and one audience per chat (ADR 0036).</summary>
internal sealed class LinkedChatConfiguration : IEntityTypeConfiguration<LinkedChat>
{
    public void Configure(EntityTypeBuilder<LinkedChat> builder)
    {
        builder.ToTable("linked_chats");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Audience).HasColumnName("audience").HasConversion<string>().HasMaxLength(16);
        builder.Property(c => c.ChatId).HasColumnName("chat_id");
        builder.Property(c => c.Title).HasColumnName("title").HasMaxLength(LinkedChat.MaxTitleLength);
        builder.Property(c => c.LinkedAtUtc).HasColumnName("linked_at_utc");
        builder.HasIndex(c => c.Audience).IsUnique().HasDatabaseName("ux_linked_chats_audience");
        builder.HasIndex(c => c.ChatId).IsUnique().HasDatabaseName("ux_linked_chats_chat_id");
    }
}

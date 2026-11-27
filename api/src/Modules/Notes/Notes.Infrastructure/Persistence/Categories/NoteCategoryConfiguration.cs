using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notes.Domain.Categories;

namespace Notes.Infrastructure.Persistence.Categories;

internal sealed class NoteCategoryConfiguration : IEntityTypeConfiguration<NoteCategory>
{
    public void Configure(EntityTypeBuilder<NoteCategory> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(NoteCategory.MaxNameLength).IsRequired();
        builder.Property(c => c.Emoji).HasColumnName("emoji").HasMaxLength(NoteCategory.MaxEmojiLength);
        builder.Property(c => c.SortOrder).HasColumnName("sort_order");
        builder.Property(c => c.Version).IsRowVersion();
        // The unique index on lower(name) is created by the Initial migration in SQL (EF cannot index an expression).

        builder.HasData(CategorySeed.Items.Select(c => new { c.Id, c.Name, c.Emoji, c.SortOrder }));
    }
}

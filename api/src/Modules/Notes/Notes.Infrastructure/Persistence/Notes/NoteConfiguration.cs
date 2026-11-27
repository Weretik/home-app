using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Infrastructure.Persistence.Notes;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public const int EnumLength = 16;

    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(n => n.About).HasColumnName("about").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(n => n.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(n => n.Title).HasColumnName("title").HasMaxLength(Note.MaxTitleLength).IsRequired();
        builder.Property(n => n.Details).HasColumnName("details").HasMaxLength(Note.MaxDetailsLength);
        builder.Property(n => n.Place).HasColumnName("place").HasMaxLength(Note.MaxPlaceLength);
        builder.Property(n => n.IsImportant).HasColumnName("is_important");
        builder.Property(n => n.CategoryId).HasColumnName("category_id");
        builder.Property(n => n.CreatedBy).HasColumnName("created_by").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(n => n.UpdatedBy).HasColumnName("updated_by").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(n => n.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(n => n.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(n => n.Version).IsRowVersion();

        // Same schema, so a real foreign key: deleting a category leaves its notes without one (R-006).
        builder.HasOne<NoteCategory>().WithMany().HasForeignKey(n => n.CategoryId).OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_notes_categories");

        builder.HasIndex(n => new { n.About, n.Kind }).HasDatabaseName("ix_notes_about_kind");
        builder.HasIndex(n => n.CategoryId).HasDatabaseName("ix_notes_category_id");
        // No trigram index for the search: the host forbids pg_trgm and the table holds hundreds of rows (ADR 0039).
    }
}

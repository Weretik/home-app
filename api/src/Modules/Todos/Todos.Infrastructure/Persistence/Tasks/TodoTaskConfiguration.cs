using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Infrastructure.Persistence.Tasks;

internal sealed class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    public const int EnumLength = 16;

    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(t => t.Title).HasColumnName("title").HasMaxLength(TodoTask.MaxTitleLength).IsRequired();
        builder.Property(t => t.Notes).HasColumnName("notes").HasMaxLength(TodoTask.MaxNotesLength);
        builder.Property(t => t.Assignee).HasColumnName("assignee").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(t => t.Author).HasColumnName("author").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(t => t.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(t => t.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(t => t.DueDate).HasColumnName("due_date");
        builder.Property(t => t.CategoryId).HasColumnName("category_id");
        builder.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(t => t.UpdatedAtUtc).HasColumnName("updated_at_utc");
        builder.Property(t => t.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(t => t.ImportKey).HasColumnName("import_key").HasMaxLength(TodoTask.MaxImportKeyLength);
        builder.Property(t => t.SourceKind).HasColumnName("source_kind").HasMaxLength(TodoTask.MaxSourceKindLength);
        builder.Property(t => t.SourceId).HasColumnName("source_id");
        builder.Property(t => t.Version).IsRowVersion();
        builder.Ignore(t => t.IsArchived);

        // The steps live inside the task: their own table, deleted with the task.
        builder.HasMany(t => t.Steps).WithOne().HasForeignKey("TaskId").OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_task_steps_tasks");
        builder.Navigation(t => t.Steps).HasField("_steps").UsePropertyAccessMode(PropertyAccessMode.Field);

        // Same schema, so a real foreign key: deleting a category leaves its tasks without one.
        builder.HasOne<TodoCategory>().WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_tasks_categories");

        builder.HasIndex(t => new { t.Status, t.DueDate }).HasDatabaseName("ix_tasks_status_due_date");
        builder.HasIndex(t => t.CompletedAtUtc).IsDescending().HasDatabaseName("ix_tasks_completed_at_utc");
        builder.HasIndex(t => t.CategoryId).HasDatabaseName("ix_tasks_category_id");
        // A plain unique index (NULLs never collide), so the import script can say ON CONFLICT (import_key).
        builder.HasIndex(t => t.ImportKey).IsUnique().HasDatabaseName("ux_tasks_import_key");
        // One task per record of another module, such as a gift (spec gifts/002); tasks without one never collide.
        builder.HasIndex(t => new { t.SourceKind, t.SourceId }).IsUnique().HasDatabaseName("ux_tasks_source")
            .HasFilter("source_kind IS NOT NULL");
        // The trigram index on lower(title) for the search is created by the Initial migration in SQL.
    }
}

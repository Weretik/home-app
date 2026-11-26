using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Infrastructure.Persistence.Items;

internal sealed class ShoppingItemConfiguration : IEntityTypeConfiguration<ShoppingItem>
{
    public void Configure(EntityTypeBuilder<ShoppingItem> builder)
    {
        builder.ToTable("shopping_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.Name).HasColumnName("name").HasMaxLength(ShoppingItem.MaxNameLength).IsRequired();
        builder.Property(i => i.NormalizedName).HasColumnName("normalized_name").HasMaxLength(ShoppingItem.MaxNameLength).IsRequired();
        builder.Property(i => i.Quantity).HasColumnName("quantity").HasMaxLength(ShoppingItem.MaxQuantityLength);
        builder.Property(i => i.Note).HasColumnName("note").HasMaxLength(ShoppingItem.MaxNoteLength);
        builder.Property(i => i.DepartmentId).HasColumnName("department_id");
        builder.Property(i => i.Urgent).HasColumnName("urgent");
        builder.Property(i => i.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.AddedBy).HasColumnName("added_by").HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.AddedAtUtc).HasColumnName("added_at_utc");
        builder.Property(i => i.BoughtBy).HasColumnName("bought_by").HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.BoughtAtUtc).HasColumnName("bought_at_utc");
        builder.Property(i => i.ClearedAtUtc).HasColumnName("cleared_at_utc");
        builder.Property(i => i.ImportKey).HasColumnName("import_key").HasMaxLength(ShoppingItem.MaxImportKeyLength);
        builder.Property(i => i.Version).IsRowVersion();

        // Same schema, so a real foreign key: a department is deleted only after its items moved to the fallback.
        builder.HasOne<Department>().WithMany().HasForeignKey(i => i.DepartmentId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_shopping_items_departments");
        // The cart window, history and frequent items all filter by status and bought time.
        builder.HasIndex(i => new { i.Status, i.BoughtAtUtc }).HasDatabaseName("ix_shopping_items_status_bought_at_utc");
        builder.HasIndex(i => i.NormalizedName).HasDatabaseName("ix_shopping_items_normalized_name");
        builder.HasIndex(i => i.DepartmentId).HasDatabaseName("ix_shopping_items_department_id");
        builder.HasIndex(i => i.ImportKey).IsUnique().HasFilter("import_key IS NOT NULL")
            .HasDatabaseName("ux_shopping_items_import_key");
    }
}

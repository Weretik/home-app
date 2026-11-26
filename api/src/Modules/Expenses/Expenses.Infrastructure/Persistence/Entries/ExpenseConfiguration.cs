using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence.Entries;

internal sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.Date).HasColumnName("date");
        builder.Property(e => e.Payer).HasColumnName("payer").HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CategoryId).HasColumnName("category_id");
        builder.Property(e => e.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(EntryRules.MaxCommentLength);
        builder.Property(e => e.OutsideLimits).HasColumnName("outside_limits");
        builder.Property(e => e.ImportKey).HasColumnName("import_key").HasMaxLength(EntryRules.MaxImportKeyLength);
        builder.Property(e => e.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(e => e.Version).IsRowVersion();

        // Same schema, so a real foreign key: a category with expenses cannot disappear.
        builder.HasOne<Category>().WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_expenses_categories");
        builder.HasIndex(e => e.Date).HasDatabaseName("ix_expenses_date");
        builder.HasIndex(e => e.CategoryId).HasDatabaseName("ix_expenses_category_id");
        builder.HasIndex(e => e.ImportKey).IsUnique().HasFilter("import_key IS NOT NULL")
            .HasDatabaseName("ux_expenses_import_key");
    }
}

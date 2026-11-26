using Expenses.Domain.Categories;
using Expenses.Domain.Planning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence.Entries;

internal sealed class PlannedExpenseConfiguration : IEntityTypeConfiguration<PlannedExpense>
{
    public void Configure(EntityTypeBuilder<PlannedExpense> builder)
    {
        builder.ToTable("planned_expenses");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.Month).HasColumnName("month");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(PlannedExpense.MaxNameLength);
        builder.Property(p => p.CategoryId).HasColumnName("category_id");
        builder.Property(p => p.Payer).HasColumnName("payer").HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(p => p.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(p => p.Version).IsRowVersion();

        // Same schema, so a real foreign key: a category with plans cannot disappear.
        builder.HasOne<Category>().WithMany().HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_planned_expenses_categories");
        builder.HasIndex(p => p.Month).HasDatabaseName("ix_planned_expenses_month");
        builder.HasIndex(p => p.CategoryId).HasDatabaseName("ix_planned_expenses_category_id");
    }
}

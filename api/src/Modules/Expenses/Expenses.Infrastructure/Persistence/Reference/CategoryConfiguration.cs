using Expenses.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence.Reference;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(Category.MaxNameLength).IsRequired();
        builder.Property(c => c.SortOrder).HasColumnName("sort_order");
        builder.HasIndex(c => c.Name).IsUnique().HasDatabaseName("ux_categories_name");

        builder.HasData(CategorySeed.Items.Select(c => new { c.Id, c.Name, c.SortOrder }));
    }
}

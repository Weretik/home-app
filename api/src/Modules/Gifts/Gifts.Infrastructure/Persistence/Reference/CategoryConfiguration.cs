using Gifts.Application.Features.Categories;
using Gifts.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gifts.Infrastructure.Persistence.Reference;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(Category.MaxNameLength).IsRequired();
        builder.Property(c => c.SortOrder).HasColumnName("sort_order");
        builder.HasIndex(c => c.Name).IsUnique().HasDatabaseName(CategoryNames.UniqueIndex);

        builder.HasData(CategorySeed.Items.Select(c => new { Id = new CategoryId(c.Id), c.Name, c.SortOrder }));
    }
}

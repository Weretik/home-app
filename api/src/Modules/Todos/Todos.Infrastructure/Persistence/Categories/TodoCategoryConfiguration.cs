using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todos.Domain.Categories;

namespace Todos.Infrastructure.Persistence.Categories;

internal sealed class TodoCategoryConfiguration : IEntityTypeConfiguration<TodoCategory>
{
    public void Configure(EntityTypeBuilder<TodoCategory> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(TodoCategory.MaxNameLength).IsRequired();
        builder.Property(c => c.Emoji).HasColumnName("emoji").HasMaxLength(TodoCategory.MaxEmojiLength);
        builder.Property(c => c.SortOrder).HasColumnName("sort_order");
        builder.Property(c => c.Version).IsRowVersion();
        // The unique index on lower(name) is created by the Initial migration in SQL (EF cannot index an expression).

        builder.HasData(CategorySeed.Items.Select(c => new { Id = new TodoCategoryId(c.Id), c.Name, c.Emoji, c.SortOrder }));
    }
}

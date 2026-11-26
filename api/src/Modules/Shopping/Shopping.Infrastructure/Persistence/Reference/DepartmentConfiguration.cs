using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopping.Domain.Departments;

namespace Shopping.Infrastructure.Persistence.Reference;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.Name).HasColumnName("name").HasMaxLength(Department.MaxNameLength).IsRequired();
        builder.Property(d => d.Icon).HasColumnName("icon").HasMaxLength(Department.MaxIconLength).IsRequired();
        builder.Property(d => d.SortOrder).HasColumnName("sort_order");
        builder.Property(d => d.IsFallback).HasColumnName("is_fallback");
        builder.Property(d => d.Version).IsRowVersion();
        builder.HasIndex(d => d.Name).IsUnique().HasDatabaseName("ux_departments_name");

        builder.HasData(DictionarySeed.Departments.Select(d => new { d.Id, d.Name, d.Icon, d.SortOrder, d.IsFallback }));
    }
}

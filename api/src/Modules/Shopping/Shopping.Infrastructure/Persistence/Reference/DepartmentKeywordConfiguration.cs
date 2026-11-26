using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopping.Domain.Departments;

namespace Shopping.Infrastructure.Persistence.Reference;

internal sealed class DepartmentKeywordConfiguration : IEntityTypeConfiguration<DepartmentKeyword>
{
    public void Configure(EntityTypeBuilder<DepartmentKeyword> builder)
    {
        builder.ToTable("department_keywords");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(k => k.Keyword).HasColumnName("keyword").HasMaxLength(DepartmentKeyword.MaxKeywordLength).IsRequired();
        builder.Property(k => k.DepartmentId).HasColumnName("department_id");
        builder.Property(k => k.Learned).HasColumnName("learned");
        builder.Property(k => k.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(k => k.Version).IsRowVersion();

        // Same schema, so a real foreign key: a department is deleted only after its keywords moved to the fallback.
        builder.HasOne<Department>().WithMany().HasForeignKey(k => k.DepartmentId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_department_keywords_departments");
        builder.HasIndex(k => k.Keyword, "ux_department_keywords_keyword").IsUnique();
        builder.HasIndex(k => k.DepartmentId).HasDatabaseName("ix_department_keywords_department_id");

        builder.HasData(DictionarySeed.Keywords.Select(k => new
        {
            k.Id, k.Keyword, k.DepartmentId, Learned = false, CreatedAtUtc = DictionarySeed.SeededAtUtc,
        }));
    }
}

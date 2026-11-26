using Expenses.Domain.Entries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence;

internal sealed class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.ToTable("incomes");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.Date).HasColumnName("date");
        builder.Property(i => i.Person).HasColumnName("person").HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(i => i.Comment).HasColumnName("comment").HasMaxLength(EntryRules.MaxCommentLength);
        builder.Property(i => i.ImportKey).HasColumnName("import_key").HasMaxLength(EntryRules.MaxImportKeyLength);
        builder.Property(i => i.CreatedAtUtc).HasColumnName("created_at_utc");

        builder.HasIndex(i => i.Date).HasDatabaseName("ix_incomes_date");
        builder.HasIndex(i => i.ImportKey).IsUnique().HasFilter("import_key IS NOT NULL")
            .HasDatabaseName("ux_incomes_import_key");
    }
}

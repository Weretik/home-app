using Expenses.Domain.Entries;
using Expenses.Domain.Savings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence.Entries;

internal sealed class SavingsMovementConfiguration : IEntityTypeConfiguration<SavingsMovement>
{
    public void Configure(EntityTypeBuilder<SavingsMovement> builder)
    {
        builder.ToTable("savings_movements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.Date).HasColumnName("date");
        builder.Property(m => m.Person).HasColumnName("person").HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Amount).HasColumnName("amount").HasPrecision(12, 2);
        builder.Property(m => m.Comment).HasColumnName("comment").HasMaxLength(EntryRules.MaxCommentLength);
        builder.Property(m => m.ImportKey).HasColumnName("import_key").HasMaxLength(EntryRules.MaxImportKeyLength);
        builder.Property(m => m.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(m => m.Version).IsRowVersion();
        builder.Ignore(m => m.Direction);

        builder.HasIndex(m => m.Date).HasDatabaseName("ix_savings_movements_date");
        builder.HasIndex(m => m.ImportKey).IsUnique().HasFilter("import_key IS NOT NULL")
            .HasDatabaseName("ux_savings_movements_import_key");
    }
}

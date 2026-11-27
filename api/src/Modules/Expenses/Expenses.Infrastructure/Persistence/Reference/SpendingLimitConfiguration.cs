using Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;
using Expenses.Domain.Budget;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence.Reference;

internal sealed class SpendingLimitConfiguration : IEntityTypeConfiguration<SpendingLimit>
{
    public void Configure(EntityTypeBuilder<SpendingLimit> builder)
    {
        builder.ToTable("spending_limits");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(l => l.Name).HasColumnName("name").HasMaxLength(SpendingLimit.MaxNameLength).IsRequired();
        builder.Property(l => l.Period).HasColumnName("period").HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Amounts).HasColumnName("amounts").HasColumnType(AmountHistoryColumn.Type)
            .HasConversion(AmountHistoryColumn.Converter, AmountHistoryColumn.Comparer);
        builder.Property(l => l.SortOrder).HasColumnName("sort_order");
        builder.Property(l => l.Version).IsRowVersion();
        builder.HasIndex(l => l.SortOrder).IsUnique().HasDatabaseName(CreateSpendingLimitHandler.SortOrderIndex);

        // A plain uuid[] column: the ids point at categories of the same schema, read as a set, never joined on.
        builder.Ignore(l => l.CategoryIds);
        builder.Property<List<Guid>>("_categoryIds").HasColumnName("category_ids").HasColumnType("uuid[]");

        // The starting limits are inserted by the Budget migration (the values of BudgetSeed): EF seed data cannot hold
        // an array column without reporting a model change on every build.
    }
}

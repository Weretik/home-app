using Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;
using Expenses.Domain.Budget;
using Expenses.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Expenses.Infrastructure.Persistence.Reference;

internal sealed class SpendingLimitConfiguration : IEntityTypeConfiguration<SpendingLimit>
{
    private static readonly ValueConverter<List<CategoryId>, List<Guid>> CategoryIdsToValues = new(
        ids => ids.Select(id => id.Value).ToList(),
        values => values.Select(value => new CategoryId(value)).ToList());

    private static readonly ValueComparer<List<CategoryId>> CategoryIdsComparer = new(
        (a, b) => a == null ? b == null : b != null && a.SequenceEqual(b),
        ids => ids.Aggregate(0, (hash, id) => HashCode.Combine(hash, id)),
        ids => ids.ToList());

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
        builder.Property<List<CategoryId>>("_categoryIds").HasColumnName("category_ids").HasColumnType("uuid[]")
            .HasConversion(CategoryIdsToValues, CategoryIdsComparer);

        // The starting limits are inserted by the Budget migration (the values of BudgetSeed): EF seed data cannot hold
        // an array column without reporting a model change on every build.
    }
}

using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Expenses.Infrastructure.Persistence.Reference;

internal sealed class BudgetSettingsConfiguration : IEntityTypeConfiguration<BudgetSettings>
{
    public void Configure(EntityTypeBuilder<BudgetSettings> builder)
    {
        builder.ToTable("budget_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.MonthlyLimits).HasColumnName("monthly_limits").HasColumnType(AmountHistoryColumn.Type)
            .HasConversion(AmountHistoryColumn.Converter, AmountHistoryColumn.Comparer);
        builder.Property(s => s.SavingsGoal).HasColumnName("savings_goal").HasPrecision(12, 2);
        builder.Property(s => s.LimitsFrom).HasColumnName("limits_from");
        builder.Property(s => s.Version).IsRowVersion();

        builder.HasData(new
        {
            Id = BudgetSettings.SingletonId,
            MonthlyLimits = AmountHistory.Start(BudgetSeed.MonthlyLimit),
            SavingsGoal = BudgetSeed.SavingsGoal,
            LimitsFrom = BudgetSettings.SheetLimitsFrom,
        });
    }
}

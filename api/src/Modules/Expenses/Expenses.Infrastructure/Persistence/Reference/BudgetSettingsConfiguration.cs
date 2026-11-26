using Expenses.Domain.Budget;
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
        builder.Property(s => s.MonthlyLimit).HasColumnName("monthly_limit").HasPrecision(12, 2);
        builder.Property(s => s.SavingsGoal).HasColumnName("savings_goal").HasPrecision(12, 2);
        builder.Property(s => s.LimitsFrom).HasColumnName("limits_from");
        builder.Property(s => s.Version).IsRowVersion();

        builder.HasData(new
        {
            Id = BudgetSettings.SingletonId,
            MonthlyLimit = BudgetSeed.MonthlyLimit,
            SavingsGoal = BudgetSeed.SavingsGoal,
            LimitsFrom = BudgetSettings.SheetLimitsFrom,
        });
    }
}

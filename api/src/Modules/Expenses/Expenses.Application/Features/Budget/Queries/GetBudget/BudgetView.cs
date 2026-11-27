using Expenses.Domain.Budget.Amounts;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>
/// The monthly limit on all expenses of the current month (0 = none) and its steps by month, oldest first, the savings
/// goal (0 = none), the day limits start counting and carrying overspend, and the category limits.
/// </summary>
public sealed record BudgetView(
    uint Version,
    decimal MonthlyLimit,
    IReadOnlyList<AmountStep> MonthlyLimits,
    decimal SavingsGoal,
    DateOnly LimitsFrom,
    IReadOnlyList<SpendingLimitItem> Limits);

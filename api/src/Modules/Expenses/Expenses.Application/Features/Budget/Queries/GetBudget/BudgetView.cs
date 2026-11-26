namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>
/// The monthly limit on all expenses (0 = none), the savings goal (0 = none), the day limits start counting and
/// carrying overspend, and the category limits.
/// </summary>
public sealed record BudgetView(
    uint Version,
    decimal MonthlyLimit,
    decimal SavingsGoal,
    DateOnly LimitsFrom,
    IReadOnlyList<SpendingLimitItem> Limits);

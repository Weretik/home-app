namespace Expenses.Api.Endpoints.Budget.Overview;

/// <summary>
/// The monthly limit on all expenses and the savings goal (0 turns either off), and the day limits start counting and
/// carrying overspend (left out: unchanged).
/// </summary>
internal sealed record BudgetSettingsRequest(decimal MonthlyLimit, decimal SavingsGoal, DateOnly? LimitsFrom = null);

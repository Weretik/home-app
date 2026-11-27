namespace Expenses.Api.Endpoints.Budget.Overview;

/// <summary>
/// The monthly limit on all expenses and the savings goal (0 turns either off), the day limits start counting and
/// carrying overspend (left out: unchanged), and a day of the first month the monthly limit applies to (left out: the
/// current month; earlier months keep theirs).
/// </summary>
internal sealed record BudgetSettingsRequest(
    decimal MonthlyLimit, decimal SavingsGoal, DateOnly? LimitsFrom = null, DateOnly? MonthlyLimitFrom = null);

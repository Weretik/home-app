namespace Expenses.Api.Endpoints.Budget.Overview;

/// <summary>The monthly limit on all expenses and the savings goal; 0 turns either off.</summary>
internal sealed record BudgetSettingsRequest(decimal MonthlyLimit, decimal SavingsGoal);

namespace Expenses.Application.Features.Planned.Queries.ListPlannedExpenses;

/// <summary>A month's planned expenses and what they add up to («Запланировано»).</summary>
public sealed record PlannedExpenseList(IReadOnlyList<PlannedExpenseItem> Items, decimal TotalAmount);

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

/// <summary>One page of a month's or a period's expenses, plus the sum of every expense that matches the filter (not just this page).</summary>
public sealed record ExpenseList(
    IReadOnlyList<ExpenseItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    decimal TotalAmount);

namespace Expenses.Application.Features.Incomes.Queries.ListIncomes;

/// <summary>One page of a month's incomes, plus the sum of every income that matches the filter.</summary>
public sealed record IncomeList(
    IReadOnlyList<IncomeItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    decimal TotalAmount);

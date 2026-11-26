namespace Expenses.Application.Features.Savings.Queries.ListSavingsMovements;

/// <summary>
/// One page of a month's savings movements, with the month's sums (<see cref="Withdrawals"/> as a positive number),
/// the balance before and after the month, and the jar's balance today.
/// </summary>
public sealed record SavingsMovementList(
    IReadOnlyList<SavingsMovementItem> Items,
    int Page,
    int PageSize,
    int TotalCount,
    decimal Deposits,
    decimal Withdrawals,
    decimal BalanceAtStart,
    decimal BalanceAtEnd,
    decimal CurrentBalance);

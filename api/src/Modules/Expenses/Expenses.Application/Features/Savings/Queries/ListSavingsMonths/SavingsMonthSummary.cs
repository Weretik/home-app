namespace Expenses.Application.Features.Savings.Queries.ListSavingsMonths;

/// <summary>One month of the jar: put in, taken out (positive) and the balance at the month's end.</summary>
public sealed record SavingsMonthSummary(int Year, int Month, decimal Deposits, decimal Withdrawals, decimal BalanceAtEnd);

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMonths;

/// <summary>The jar's balance today and its movement month by month, newest month first.</summary>
public sealed record SavingsOverview(decimal CurrentBalance, IReadOnlyList<SavingsMonthSummary> Months);

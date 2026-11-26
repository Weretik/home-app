namespace Expenses.Contracts;

/// <summary>A spending limit in its current month or quarter, with the carried-in overspend counted.</summary>
/// <param name="Quarterly">True for a quarterly limit, false for a monthly one.</param>
public sealed record LimitState(string Name, bool Quarterly, decimal Amount, decimal Spent, decimal Left, int Percent);

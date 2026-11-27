namespace Expenses.Domain.Budget.Amounts;

/// <summary>
/// One step of a limit's amounts: the amount that applies from <paramref name="From"/> (the first day of a month, or
/// of a quarter for a quarterly limit) until the next step.
/// </summary>
public sealed record AmountStep(DateOnly From, decimal Amount);

namespace Expenses.Contracts;

/// <summary>Expenses of a few days: the total, by payer, and the five biggest categories.</summary>
public sealed record ExpensesPeriodReport(
    DateOnly From,
    DateOnly To,
    decimal Total,
    int Count,
    IReadOnlyList<NamedAmount> ByPayer,
    IReadOnlyList<NamedAmount> TopCategories);

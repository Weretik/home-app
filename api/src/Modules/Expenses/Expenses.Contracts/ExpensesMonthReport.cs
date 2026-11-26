namespace Expenses.Contracts;

/// <summary>One month in figures, like the site's month summary.</summary>
/// <param name="MonthlyLimit">The limit on all expenses of a month; 0 when there is none.</param>
/// <param name="Result">Incomes minus expenses minus what went into the savings jar.</param>
public sealed record ExpensesMonthReport(
    int Year,
    int Month,
    decimal Incomes,
    IReadOnlyList<NamedAmount> IncomesByPerson,
    decimal Expenses,
    IReadOnlyList<NamedAmount> ExpensesByPayer,
    decimal MonthlyLimit,
    decimal CarriedIn,
    decimal Result,
    decimal SavingsNet,
    decimal SavingsBalance,
    decimal SavingsGoal,
    IReadOnlyList<LimitState> Limits,
    IReadOnlyList<NamedAmount> TopCategories);

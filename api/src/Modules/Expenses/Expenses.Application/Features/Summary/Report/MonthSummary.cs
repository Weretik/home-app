namespace Expenses.Application.Features.Summary.Report;

/// <summary>
/// One month at a glance, like the old «Сводка» sheet without its plan block: incomes and expenses with who they
/// belong to, the month's result after the savings jar, the jar itself, the spending limits and the top categories.
/// </summary>
/// <param name="Result">Incomes minus expenses minus what was put into the jar (plus what was taken out).</param>
/// <param name="MonthlyLimit">The limit on all expenses of a month; 0 means none.</param>
/// <param name="DaysLeft">Days left in the month counting today, or null when the month is not the current one.</param>
/// <param name="SavingsNet">Put into the jar minus taken out during the month.</param>
/// <param name="SavingsBalance">What the jar held at the end of the month.</param>
/// <param name="SavingsGoal">How much the jar should hold; 0 means no goal.</param>
public sealed record MonthSummary(
    int Year,
    int Month,
    decimal Incomes,
    IReadOnlyList<PersonAmount> IncomesByPerson,
    decimal Expenses,
    IReadOnlyList<PayerAmount> ExpensesByPayer,
    decimal MonthlyLimit,
    int? DaysLeft,
    decimal Result,
    decimal SavingsNet,
    decimal SavingsBalance,
    decimal SavingsGoal,
    IReadOnlyList<LimitUsage> Limits,
    IReadOnlyList<CategoryAmount> TopCategories);

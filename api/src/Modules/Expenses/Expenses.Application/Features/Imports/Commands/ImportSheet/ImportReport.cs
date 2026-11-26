namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

/// <summary>
/// What an import did: rows added and rows skipped because an earlier import already stored them, and the totals of
/// the imported tabs by month, to compare with the sheet.
/// </summary>
public sealed record ImportReport(
    int ExpensesAdded,
    int ExpensesSkipped,
    int IncomesAdded,
    int IncomesSkipped,
    int SavingsAdded,
    int SavingsSkipped,
    IReadOnlyList<ImportMonth> Months);

/// <summary>The sums of one month in the imported tabs.</summary>
/// <param name="Savings">The month's net savings movement: put in minus taken out.</param>
public sealed record ImportMonth(string Month, decimal Expenses, decimal Incomes, decimal Savings);

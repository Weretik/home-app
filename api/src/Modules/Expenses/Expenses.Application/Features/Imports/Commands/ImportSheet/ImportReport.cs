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

namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

/// <summary>The sums of one month in the imported tabs.</summary>
/// <param name="Savings">The month's net savings movement: put in minus taken out.</param>
public sealed record ImportMonth(string Month, decimal Expenses, decimal Incomes, decimal Savings);

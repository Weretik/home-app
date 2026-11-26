using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

/// <summary>
/// Loads the «Расходы», «Доходы» and (when given) «Копилка» tabs of the old Google Sheet into the module. Each tab is its rows of cell text
/// exactly as the sheet shows them, header first. Running it again with the same rows adds nothing.
/// </summary>
public sealed record ImportSheetCommand(
    IReadOnlyList<IReadOnlyList<string>> ExpenseRows,
    IReadOnlyList<IReadOnlyList<string>> IncomeRows,
    IReadOnlyList<IReadOnlyList<string>>? SavingsRows = null) : ICommand<Result<ImportReport>>;

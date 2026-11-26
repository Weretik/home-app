using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

/// <summary>
/// Loads the «Расходы» and «Доходы» tabs of the old Google Sheet into the module. Each tab is its rows of cell text
/// exactly as the sheet shows them, header first. Running it again with the same rows adds nothing.
/// </summary>
public sealed record ImportSheetCommand(
    IReadOnlyList<IReadOnlyList<string>> ExpenseRows,
    IReadOnlyList<IReadOnlyList<string>> IncomeRows) : ICommand<Result<ImportReport>>;

using FluentValidation;

namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

internal sealed class ImportSheetValidator : AbstractValidator<ImportSheetCommand>
{
    public const int MaxRows = 10_000;
    public const int MaxCells = 20;
    public const int MaxCellLength = 1_000;

    public ImportSheetValidator()
    {
        RuleFor(c => c.ExpenseRows).NotEmpty().Must(rows => rows.Count <= MaxRows)
            .WithMessage($"A tab has at most {MaxRows} rows.");
        RuleFor(c => c.IncomeRows).NotEmpty().Must(rows => rows.Count <= MaxRows)
            .WithMessage($"A tab has at most {MaxRows} rows.");
        RuleFor(c => c.SavingsRows).Must(rows => rows is null || rows.Count <= MaxRows)
            .WithMessage($"A tab has at most {MaxRows} rows.");
        RuleForEach(c => c.SavingsRows).Must(BeAReasonableRow)
            .WithMessage($"A row has at most {MaxCells} cells of at most {MaxCellLength} characters.");
        RuleForEach(c => c.ExpenseRows).Must(BeAReasonableRow)
            .WithMessage($"A row has at most {MaxCells} cells of at most {MaxCellLength} characters.");
        RuleForEach(c => c.IncomeRows).Must(BeAReasonableRow)
            .WithMessage($"A row has at most {MaxCells} cells of at most {MaxCellLength} characters.");
    }

    private static bool BeAReasonableRow(IReadOnlyList<string> row) =>
        row is not null && row.Count <= MaxCells && row.All(cell => cell is null || cell.Length <= MaxCellLength);
}

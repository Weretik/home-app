namespace Expenses.Application.Features.Imports.SheetFormat;

/// <summary>
/// Reads the «Расходы» and «Доходы» tabs, given as rows of cell text with the header first. The header must match,
/// so a moved column cannot load amounts as dates. Empty rows are skipped; every unreadable cell is reported with
/// its sheet row number, and nothing is imported while any is left.
/// </summary>
internal static class SheetReader
{
    public static readonly string[] ExpenseHeader = ["Дата", "Кто", "Категория", "Сумма, грн", "Оплата", "Комментарий"];
    public static readonly string[] IncomeHeader = ["Дата", "Кто", "Сумма, грн", "Комментарий"];

    public static SheetRead<SheetExpense> ReadExpenses(IReadOnlyList<IReadOnlyList<string>> rows) =>
        Read(rows, ExpenseHeader, "expenseRows", (row, cells, problems) =>
        {
            var date = SheetValues.ParseDate(cells[0]);
            var payer = SheetValues.ParsePayer(cells[1]);
            var category = cells[2].Trim();
            var amount = SheetValues.ParseAmount(cells[3]);
            var method = SheetValues.ParsePaymentMethod(cells[4]);

            if (date is null) problems.Add($"Row {row}: «{cells[0]}» is not a date like 01.10.2026.");
            if (payer is null) problems.Add($"Row {row}: «{cells[1]}» is not Виталий, Мария or Общее.");
            if (category.Length == 0) problems.Add($"Row {row}: the category is empty.");
            if (amount is null) problems.Add($"Row {row}: «{cells[3]}» is not an amount like 2,424.00.");
            if (method is null) problems.Add($"Row {row}: «{cells[4]}» is not Карта or Наличные.");

            return date is null || payer is null || category.Length == 0 || amount is null || method is null
                ? null
                : new SheetExpense(row, date.Value, payer.Value, category, amount.Value, method.Value, cells[5]);
        });

    public static SheetRead<SheetIncome> ReadIncomes(IReadOnlyList<IReadOnlyList<string>> rows) =>
        Read(rows, IncomeHeader, "incomeRows", (row, cells, problems) =>
        {
            var date = SheetValues.ParseDate(cells[0]);
            var person = SheetValues.ParsePerson(cells[1]);
            var amount = SheetValues.ParseAmount(cells[2]);

            if (date is null) problems.Add($"Row {row}: «{cells[0]}» is not a date like 01.10.2026.");
            if (person is null) problems.Add($"Row {row}: «{cells[1]}» is not Виталий or Мария.");
            if (amount is null) problems.Add($"Row {row}: «{cells[2]}» is not an amount like 2,424.00.");

            return date is null || person is null || amount is null
                ? null
                : new SheetIncome(row, date.Value, person.Value, amount.Value, cells[3]);
        });

    private static SheetRead<T> Read<T>(
        IReadOnlyList<IReadOnlyList<string>> rows,
        string[] header,
        string field,
        Func<int, string[], List<string>, T?> readRow)
        where T : class
    {
        var problems = new List<string>();
        var items = new List<T>();

        if (rows.Count == 0 || !HeaderMatches(rows[0], header))
        {
            problems.Add($"The first row must be the header: {string.Join(" | ", header)}.");
            return new SheetRead<T>(items, field, problems);
        }

        for (var index = 1; index < rows.Count; index++)
        {
            var cells = Pad(rows[index], header.Length);
            if (cells.All(string.IsNullOrWhiteSpace))
                continue;

            // The sheet numbers rows from 1 and the header is row 1.
            if (readRow(index + 1, cells, problems) is { } item)
                items.Add(item);
        }

        return new SheetRead<T>(items, field, problems);
    }

    private static bool HeaderMatches(IReadOnlyList<string> row, string[] header) =>
        row.Count >= header.Length
        && header.Select((title, i) => string.Equals(row[i].Trim(), title, StringComparison.OrdinalIgnoreCase)).All(x => x);

    /// <summary>The Sheets API drops trailing empty cells, so a row without a comment is one cell short.</summary>
    private static string[] Pad(IReadOnlyList<string> row, int length) =>
        Enumerable.Range(0, length).Select(i => i < row.Count ? row[i] ?? string.Empty : string.Empty).ToArray();
}

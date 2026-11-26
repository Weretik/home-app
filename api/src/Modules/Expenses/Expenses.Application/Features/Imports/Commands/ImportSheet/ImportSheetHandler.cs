using System.Globalization;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Application.Features.Imports.SheetFormat;
using Expenses.Application.Features.Imports.SheetFormat.Rows;
using Expenses.Domain.Entries;
using Expenses.Domain.Savings;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Imports.Commands.ImportSheet;

/// <summary>
/// All or nothing: every row is read and checked first, and any problem stops the import with a message per row.
/// Rows whose import key is already stored are skipped, the rest is added, and everything is saved once.
/// </summary>
public sealed class ImportSheetHandler(IExpensesDbContext db, TimeProvider clock)
    : ICommandHandler<ImportSheetCommand, Result<ImportReport>>
{
    public async ValueTask<Result<ImportReport>> Handle(ImportSheetCommand command, CancellationToken cancellationToken)
    {
        var expenses = SheetReader.ReadExpenses(command.ExpenseRows);
        var incomes = SheetReader.ReadIncomes(command.IncomeRows);
        var savings = command.SavingsRows is null
            ? new SheetRead<SheetSavings>([], "savingsRows", [])
            : SheetReader.ReadSavings(command.SavingsRows);
        var problems = new Dictionary<string, List<string>>(StringComparer.Ordinal)
        {
            [expenses.Field] = [.. expenses.Problems],
            [incomes.Field] = [.. incomes.Problems],
            [savings.Field] = [.. savings.Problems],
        };

        var categories = await db.Categories.AsNoTracking()
            .ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var row in expenses.Items.Where(e => !categories.ContainsKey(e.CategoryName)))
            problems[expenses.Field].Add($"Row {row.Row}: there is no category «{row.CategoryName}».");

        var expenseKeys = ImportKeys.For("expense", expenses.Items, ImportKeys.Expense);
        var incomeKeys = ImportKeys.For("income", incomes.Items, ImportKeys.Income);
        var savingsKeys = ImportKeys.For("savings", savings.Items, ImportKeys.Savings);
        var storedSavingsKeys = await db.SavingsMovements
            .Where(m => m.ImportKey != null && savingsKeys.Contains(m.ImportKey))
            .Select(m => m.ImportKey!)
            .ToListAsync(cancellationToken);
        var storedExpenseKeys = await db.Expenses
            .Where(e => e.ImportKey != null && expenseKeys.Contains(e.ImportKey))
            .Select(e => e.ImportKey!)
            .ToListAsync(cancellationToken);
        var storedIncomeKeys = await db.Incomes
            .Where(i => i.ImportKey != null && incomeKeys.Contains(i.ImportKey))
            .Select(i => i.ImportKey!)
            .ToListAsync(cancellationToken);

        var now = clock.GetUtcNow();
        var newExpenses = new List<Expense>();
        for (var i = 0; i < expenses.Items.Count; i++)
        {
            var row = expenses.Items[i];
            if (storedExpenseKeys.Contains(expenseKeys[i]) || !categories.TryGetValue(row.CategoryName, out var categoryId))
                continue;

            var created = Expense.Create(
                row.Date, row.Payer, categoryId, row.Amount, row.PaymentMethod, row.Comment, now, expenseKeys[i]);
            if (created.IsSuccess)
                newExpenses.Add(created.Value);
            else
                problems[expenses.Field].Add($"Row {row.Row}: {created.Error!.Message}");
        }

        var newIncomes = new List<Income>();
        for (var i = 0; i < incomes.Items.Count; i++)
        {
            var row = incomes.Items[i];
            if (storedIncomeKeys.Contains(incomeKeys[i]))
                continue;

            var created = Income.Create(row.Date, row.Person, row.Amount, row.Comment, now, incomeKeys[i]);
            if (created.IsSuccess)
                newIncomes.Add(created.Value);
            else
                problems[incomes.Field].Add($"Row {row.Row}: {created.Error!.Message}");
        }

        var newSavings = new List<SavingsMovement>();
        for (var i = 0; i < savings.Items.Count; i++)
        {
            var row = savings.Items[i];
            if (storedSavingsKeys.Contains(savingsKeys[i]))
                continue;

            var created = SavingsMovement.Create(row.Date, row.Person, row.Amount, row.Comment, now, savingsKeys[i]);
            if (created.IsSuccess)
                newSavings.Add(created.Value);
            else
                problems[savings.Field].Add($"Row {row.Row}: {created.Error!.Message}");
        }

        if (problems.Values.Any(list => list.Count > 0))
            return Invalid(problems);

        db.Expenses.AddRange(newExpenses);
        db.Incomes.AddRange(newIncomes);
        db.SavingsMovements.AddRange(newSavings);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            return ImportErrors.AlreadyRunning;
        }

        return new ImportReport(
            newExpenses.Count,
            expenses.Items.Count - newExpenses.Count,
            newIncomes.Count,
            incomes.Items.Count - newIncomes.Count,
            newSavings.Count,
            savings.Items.Count - newSavings.Count,
            Months(expenses.Items, incomes.Items, savings.Items));
    }

    private static Error Invalid(Dictionary<string, List<string>> problems)
    {
        var fields = problems.Where(p => p.Value.Count > 0).ToDictionary(p => p.Key, p => p.Value.ToArray(), StringComparer.Ordinal);
        var count = fields.Values.Sum(messages => messages.Length);
        return new Error(
            ImportErrors.InvalidRowsCode,
            $"Nothing was imported: {count} problem(s) in the sheet rows.",
            ErrorType.Validation,
            fields);
    }

    private static List<ImportMonth> Months(
        IReadOnlyList<SheetExpense> expenses, IReadOnlyList<SheetIncome> incomes, IReadOnlyList<SheetSavings> savings)
    {
        var spent = expenses.GroupBy(e => MonthOf(e.Date)).ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        var earned = incomes.GroupBy(i => MonthOf(i.Date)).ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));
        var saved = savings.GroupBy(s => MonthOf(s.Date)).ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));
        return spent.Keys.Union(earned.Keys).Union(saved.Keys)
            .Order(StringComparer.Ordinal)
            .Select(month => new ImportMonth(
                month, spent.GetValueOrDefault(month), earned.GetValueOrDefault(month), saved.GetValueOrDefault(month)))
            .ToList();
    }

    private static string MonthOf(DateOnly date) => date.ToString("yyyy-MM", CultureInfo.InvariantCulture);
}

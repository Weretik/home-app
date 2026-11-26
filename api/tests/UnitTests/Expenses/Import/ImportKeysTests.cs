using Expenses.Application.Features.Imports.SheetFormat;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace UnitTests.Expenses.Import;

public sealed class ImportKeysTests
{
    private static SheetExpense Bill(int row, decimal amount) =>
        new(row, new DateOnly(2026, 10, 21), Payer.Maria, "Коммуналка", amount, PaymentMethod.Card, "Харківводоканал (*6414)");

    [Fact]
    public void Identical_Rows_GetDifferentKeys_ByOccurrence()
    {
        var keys = ImportKeys.For("expense", [Bill(2, 17.62m), Bill(3, 17.62m), Bill(4, 18.26m)], ImportKeys.Expense);

        Assert.Equal(3, keys.Distinct().Count());
        Assert.EndsWith(":1", keys[0], StringComparison.Ordinal);
        Assert.EndsWith(":2", keys[1], StringComparison.Ordinal);
        Assert.EndsWith(":1", keys[2], StringComparison.Ordinal);
    }

    [Fact]
    public void The_Same_Rows_GiveTheSameKeys_OnEveryRun_RegardlessOfRowNumbers()
    {
        var first = ImportKeys.For("expense", [Bill(2, 17.62m)], ImportKeys.Expense);
        var second = ImportKeys.For("expense", [Bill(40, 17.62m)], ImportKeys.Expense);

        Assert.Equal(first, second);
        Assert.True(first[0].Length <= EntryRules.MaxImportKeyLength);
    }
}

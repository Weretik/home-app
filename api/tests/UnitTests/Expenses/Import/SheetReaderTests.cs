using Expenses.Application.Features.Imports.SheetFormat;
using Expenses.Application.Features.Imports.SheetFormat.Rows;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace UnitTests.Expenses.Import;

public sealed class SheetReaderTests
{
    private static readonly string[] ExpenseHeader = ["Дата", "Кто", "Категория", "Сумма, грн", "Оплата", "Комментарий"];
    private static readonly string[] IncomeHeader = ["Дата", "Кто", "Сумма, грн", "Комментарий"];

    [Fact]
    public void Expense_Rows_AreRead_AsTheSheetShowsThem()
    {
        var read = SheetReader.ReadExpenses(
        [
            ExpenseHeader,
            ["01.10.2026", "Мария", "Личные Маша", "2,424.00", "Карта", "Снятие наличных (*6414)"],
            ["01.11.2026", "Общее", "Продукты", "413.31", "Наличные"],
        ]);

        Assert.Empty(read.Problems);
        Assert.Equal(2, read.Items.Count);
        var first = read.Items[0];
        Assert.Equal(new SheetExpense(2, new DateOnly(2026, 10, 1), Payer.Maria, "Личные Маша", 2424.00m, PaymentMethod.Card,
            "Снятие наличных (*6414)"), first);
        Assert.Equal(Payer.Shared, read.Items[1].Payer);
        Assert.Equal(PaymentMethod.Cash, read.Items[1].PaymentMethod);
        Assert.Equal(string.Empty, read.Items[1].Comment);
    }

    [Fact]
    public void Empty_Rows_AreSkipped_AndRowNumbers_FollowTheSheet()
    {
        var read = SheetReader.ReadIncomes(
        [
            IncomeHeader,
            [],
            ["", "", "", ""],
            ["06.10.2026", "Мария", "25,025.00", "зарплата"],
        ]);

        Assert.Equal(4, Assert.Single(read.Items).Row);
    }

    [Fact]
    public void Every_UnreadableCell_IsReported_WithItsRow()
    {
        var read = SheetReader.ReadExpenses(
        [
            ExpenseHeader,
            ["2026-10-01", "Петя", "", "много", "Чек", ""],
        ]);

        Assert.Empty(read.Items);
        Assert.Equal(5, read.Problems.Count);
        Assert.All(read.Problems, problem => Assert.StartsWith("Row 2:", problem, StringComparison.Ordinal));
    }

    [Fact]
    public void A_Changed_Header_StopsTheRead()
    {
        var read = SheetReader.ReadIncomes(
        [
            ["Дата", "Сумма, грн", "Кто", "Комментарий"],
            ["06.10.2026", "25,025.00", "Мария", ""],
        ]);

        Assert.Empty(read.Items);
        Assert.Contains("header", Assert.Single(read.Problems), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("2,424.00", 2424.00)]
    [InlineData("170.00", 170.00)]
    [InlineData("23,872.31", 23872.31)]
    [InlineData("1 000,00", null)]
    [InlineData("-5", null)]
    [InlineData("12,34", null)]
    [InlineData("1.234", null)]
    public void Amounts_UseCommaForThousands_AndPointForKopecks(string cell, double? expected)
    {
        Assert.Equal(expected is null ? null : (decimal)expected, SheetValues.ParseAmount(cell));
    }

    [Theory]
    [InlineData("35,124.17", 35124.17)]
    [InlineData("-111.10", -111.10)]
    [InlineData("\u22129,399.60", -9399.60)]
    [InlineData("+500", 500.0)]
    [InlineData("- 5", -5.0)]
    [InlineData("--5", null)]
    [InlineData("1 000,00", null)]
    public void Savings_Amounts_MayCarryASign(string cell, double? expected)
    {
        Assert.Equal(expected is null ? null : (decimal)expected, SheetValues.ParseSignedAmount(cell));
    }

    [Fact]
    public void Savings_Rows_AreRead_WithTheirSign()
    {
        var read = SheetReader.ReadSavings(
        [
            ["Дата", "Кто", "Сумма, грн (+ отложили, - взяли)", "Комментарий"],
            ["30.09.2026", "Виталий", "35,124.17", "остаток на начало учёта"],
            ["03.10.2026", "Виталий", "-111.10"],
        ]);

        Assert.Empty(read.Problems);
        Assert.Equal(
            [
                new SheetSavings(2, new DateOnly(2026, 9, 30), Person.Vitaliy, 35124.17m, "остаток на начало учёта"),
                new SheetSavings(3, new DateOnly(2026, 10, 3), Person.Vitaliy, -111.10m, string.Empty),
            ],
            read.Items);
    }
}

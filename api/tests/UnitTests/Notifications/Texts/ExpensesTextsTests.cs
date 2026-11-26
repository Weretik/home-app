using Expenses.Contracts;
using Notifications.Application.Digests.Texts;

namespace UnitTests.Notifications.Texts;

public sealed class ExpensesTextsTests
{
    private static readonly ExpensesMonthReport November = new(
        2026, 11,
        Incomes: 80_000m, IncomesByPerson: [new("Vitaliy", 50_000m), new("Maria", 30_000m)],
        Expenses: 42_500.5m, ExpensesByPayer: [new("Vitaliy", 30_000m), new("Maria", 12_500.5m)],
        MonthlyLimit: 50_000m, CarriedIn: 2_500m, Result: 37_499.5m,
        SavingsNet: 5_000m, SavingsBalance: 25_000m, SavingsGoal: 100_000m,
        Limits: [new("Продукты", false, 15_000m, 13_500m, 1_500m, 90), new("Одежда", true, 10_000m, 2_000m, 8_000m, 20)],
        TopCategories: [new("Продукты", 13_500m)]);

    [Fact]
    public void The_Week_ShowsTotalsPayersTopAndHotLimits()
    {
        var week = new ExpensesPeriodReport(new DateOnly(2026, 11, 23), new DateOnly(2026, 11, 29), 9_870m, 14, [new("Maria", 9_870m)], [new("Продукты", 6_000m)]);

        var text = ExpensesTexts.Week(week, November, "https://web.example/expenses/forecast");

        Assert.StartsWith("📊 <b>Расходы за неделю</b> 23.11–29.11\nВсего: <b>9 870 ₴</b> (14 зап.)\nМаша — 9 870 ₴\n", text);
        Assert.Contains("• Продукты — 6 000 ₴", text);
        Assert.Contains("из 50 000 ₴ (90%)", text);
        Assert.Contains("⚠️ Лимиты: Продукты — 90%", text);
        Assert.DoesNotContain("Одежда", text);
    }

    [Fact]
    public void The_Month_ShowsIncomesExpensesResultSavingsAndEveryLimit()
    {
        var text = ExpensesTexts.Month(November, "https://web.example/expenses/forecast");

        Assert.StartsWith("🗓 <b>Итоги за ", text);
        Assert.Contains("➡️ Итог месяца: <b>+37 499,50 ₴</b>", text);
        Assert.Contains("🐷 Копилка: +5 000 ₴ за месяц, всего 25 000 ₴ из 100 000 ₴", text);
        Assert.Contains("⚠️ Продукты: 13 500 ₴ из 15 000 ₴ (90%)", text);
        Assert.Contains("✅ Одежда (кв.): 2 000 ₴ из 10 000 ₴ (20%)", text);
    }
}

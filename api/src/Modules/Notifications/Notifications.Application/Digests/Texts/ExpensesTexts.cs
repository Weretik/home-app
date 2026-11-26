using System.Text;
using Expenses.Contracts;

namespace Notifications.Application.Digests.Texts;

/// <summary>The weekly and the monthly expenses summaries (spec notifications/001, R-012 and R-013).</summary>
public static class ExpensesTexts
{
    /// <summary>Limits at or above this share are named in the weekly message.</summary>
    public const int WarnPercent = 80;

    public static string Week(ExpensesPeriodReport week, ExpensesMonthReport month, string budgetUrl)
    {
        var text = new StringBuilder(
            $"📊 <b>Расходы за неделю</b> {TextFormat.Day(week.From)}–{TextFormat.Day(week.To)}\n");
        text.Append($"Всего: <b>{TextFormat.Money(week.Total)}</b> ({week.Count} зап.)\n");
        AppendPayers(text, week.ByPayer);
        AppendTop(text, week.TopCategories);

        text.Append($"\n<b>{Capital(TextFormat.MonthName(month.Month))} пока:</b> {TextFormat.Money(month.Expenses)}");
        if (month.MonthlyLimit > 0)
            text.Append($" из {TextFormat.Money(month.MonthlyLimit)} ({Percent(month.Expenses + month.CarriedIn, month.MonthlyLimit)}%)");
        text.Append('\n');

        var hot = month.Limits.Where(l => l.Percent >= WarnPercent).ToList();
        if (hot.Count > 0)
            text.Append("⚠️ Лимиты: ").Append(string.Join(", ", hot.Select(l => $"{TextFormat.Escape(l.Name)} — {l.Percent}%"))).Append('\n');

        text.Append('\n').Append(TextFormat.Link(budgetUrl, "Открыть бюджет"));
        return text.ToString();
    }

    public static string Month(ExpensesMonthReport m, string budgetUrl)
    {
        var text = new StringBuilder($"🗓 <b>Итоги за {TextFormat.MonthName(m.Month)} {m.Year}</b>\n\n");
        text.Append($"💰 Доходы: <b>{TextFormat.Money(m.Incomes)}</b>{Split(m.IncomesByPerson)}\n");
        text.Append($"💸 Расходы: <b>{TextFormat.Money(m.Expenses)}</b>{Split(m.ExpensesByPayer)}\n");
        text.Append($"➡️ Итог месяца: <b>{TextFormat.Signed(m.Result)}</b>\n");
        text.Append($"🐷 Копилка: {TextFormat.Signed(m.SavingsNet)} за месяц, всего {TextFormat.Money(m.SavingsBalance)}");
        if (m.SavingsGoal > 0)
            text.Append($" из {TextFormat.Money(m.SavingsGoal)}");
        text.Append('\n');

        if (m.Limits.Count > 0)
        {
            text.Append("\n<b>Лимиты</b>\n");
            TextFormat.AppendLines(text, [.. m.Limits.Select(l =>
                $"{Mark(l.Percent)} {TextFormat.Escape(l.Name)}{(l.Quarterly ? " (кв.)" : string.Empty)}: "
                + $"{TextFormat.Money(l.Spent)} из {TextFormat.Money(l.Amount)} ({l.Percent}%)")]);
        }

        AppendTop(text, m.TopCategories);
        text.Append('\n').Append(TextFormat.Link(budgetUrl, "Открыть бюджет"));
        return text.ToString();
    }

    private static void AppendPayers(StringBuilder text, IReadOnlyList<NamedAmount> payers)
    {
        if (payers.Count > 0)
            text.Append(string.Join(" · ", payers.Select(p => $"{TextFormat.Person(p.Name)} — {TextFormat.Money(p.Amount)}"))).Append('\n');
    }

    private static void AppendTop(StringBuilder text, IReadOnlyList<NamedAmount> categories)
    {
        if (categories.Count == 0)
            return;
        text.Append("\n<b>Больше всего</b>\n");
        TextFormat.AppendLines(text, [.. categories.Select(c => $"• {TextFormat.Escape(c.Name)} — {TextFormat.Money(c.Amount)}")]);
    }

    private static string Split(IReadOnlyList<NamedAmount> parts) =>
        parts.Count == 0 ? string.Empty : " (" + string.Join(", ", parts.Select(p => $"{TextFormat.Person(p.Name)} {TextFormat.Money(p.Amount)}")) + ")";

    private static string Mark(int percent) => percent switch
    {
        > 100 => "🔴",
        >= WarnPercent => "⚠️",
        _ => "✅",
    };

    private static int Percent(decimal spent, decimal limit) => (int)Math.Round(spent / limit * 100, MidpointRounding.AwayFromZero);

    private static string Capital(string word) => char.ToUpperInvariant(word[0]) + word[1..];
}

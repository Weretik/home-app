using System.Globalization;
using Notifications.Domain.Deliveries;

namespace Notifications.Domain.Schedule;

/// <summary>
/// When each digest goes out, in Kyiv time (spec notifications/001, design «Schedule»). A digest is due while the
/// local time is inside its window; a tick outside it plans nothing, so a missed window is skipped, not sent late.
/// The single source of the times: the site shows <see cref="Windows"/>.
/// </summary>
public static class DigestSchedule
{
    public const int MorningFrom = 8;
    public const int MorningTo = 12;
    public const int EveningFrom = 18;
    public const int EveningTo = 22;
    public const int SummaryFrom = 20;
    public const int MonthCatchUpTo = 12;

    public static IReadOnlyList<DigestWindow> Windows { get; } =
    [
        new(DigestKind.TodosMorning, "Дела на сегодня — каждому лично", "каждый день в 8:00"),
        new(DigestKind.TodosCommon, "Общие: что не закрыто и что просрочено", "каждый день в 8:00, после личных"),
        new(DigestKind.GiftReminder, "Купить подарок — тому, кто дарит", "за 14 дней до даты «Купить до», в 8:00"),
        new(DigestKind.ShoppingEvening, "Что осталось в списке покупок", "каждый вечер в 18:00, если список не пуст"),
        new(DigestKind.ExpensesWeek, "Расходы за неделю", "воскресенье в 20:00"),
        new(DigestKind.ExpensesMonth, "Полная сводка за месяц", "последний день месяца в 20:00"),
    ];

    /// <summary>The digests due at <paramref name="kyivNow"/>, the local time in Kyiv.</summary>
    public static IReadOnlyList<DueSlot> DueAt(DateTime kyivNow)
    {
        var today = DateOnly.FromDateTime(kyivNow);
        var hour = kyivNow.Hour;
        var due = new List<DueSlot>();

        if (hour is >= MorningFrom and < MorningTo)
        {
            due.Add(Day(DigestKind.TodosMorning, today));
            due.Add(Day(DigestKind.TodosCommon, today));
            due.Add(Day(DigestKind.GiftReminder, today));
        }

        if (hour is >= EveningFrom and < EveningTo)
            due.Add(Day(DigestKind.ShoppingEvening, today));

        if (today.DayOfWeek == DayOfWeek.Sunday && hour >= SummaryFrom)
        {
            var monday = today.AddDays(-6);
            due.Add(new DueSlot(DigestKind.ExpensesWeek, Format(monday), monday, today));
        }

        var isLastDay = today.AddDays(1).Day == 1;
        if (isLastDay && hour >= SummaryFrom)
            due.Add(Month(today));
        else if (today.Day == 1 && hour < MonthCatchUpTo)
            due.Add(Month(today.AddDays(-1)));

        return due;
    }

    private static DueSlot Day(DigestKind kind, DateOnly day) => new(kind, Format(day), day, day);

    private static DueSlot Month(DateOnly dayInMonth)
    {
        var first = new DateOnly(dayInMonth.Year, dayInMonth.Month, 1);
        return new DueSlot(
            DigestKind.ExpensesMonth,
            first.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            first,
            first.AddMonths(1).AddDays(-1));
    }

    private static string Format(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

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
        new(DigestKind.TodosMorning, "Справи на сьогодні — кожному особисто", "щодня о 8:00"),
        new(DigestKind.TodosCommon, "Спільні: що не закрито і що прострочено", "щодня о 8:00, після особистих"),
        new(DigestKind.GiftReminder, "Купити подарунок — тому, хто дарує", "за 14 днів до дати «Купити до», о 8:00"),
        new(DigestKind.ShoppingEvening, "Що лишилося в списку покупок", "щовечора о 18:00, якщо список не порожній"),
        new(DigestKind.ExpensesWeek, "Витрати за тиждень", "щонеділі о 20:00"),
        new(DigestKind.ExpensesMonth, "Повний підсумок за місяць", "в останній день місяця о 20:00"),
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

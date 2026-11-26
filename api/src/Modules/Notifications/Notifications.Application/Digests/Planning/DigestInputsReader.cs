using Expenses.Contracts;
using Gifts.Contracts;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Schedule;
using Shopping.Contracts;
using Todos.Contracts;

namespace Notifications.Application.Digests.Planning;

/// <summary>Reads, through the other modules' contracts, only what the due digests need.</summary>
public sealed class DigestInputsReader(ITodosDigest todos, IGiftsDigest gifts, IShoppingDigest shopping, IExpensesDigest expenses)
{
    /// <summary>A gift is reminded of from this many days before its buy-by date.</summary>
    public const int GiftReminderDays = 14;

    public async Task<DigestInputs> ReadAsync(IReadOnlyCollection<DueSlot> slots, DateOnly today, CancellationToken cancellationToken)
    {
        bool Due(DigestKind kind) => slots.Any(s => s.Kind == kind);
        var week = slots.FirstOrDefault(s => s.Kind == DigestKind.ExpensesWeek);
        var month = slots.FirstOrDefault(s => s.Kind == DigestKind.ExpensesMonth);

        return new DigestInputs(
            Todos: Due(DigestKind.TodosMorning) || Due(DigestKind.TodosCommon) ? await todos.ListActiveAsync(cancellationToken) : null,
            Gifts: Due(DigestKind.GiftReminder)
                ? await gifts.ListPurchasesDueAsync(today, today.AddDays(GiftReminderDays), cancellationToken)
                : null,
            Shopping: Due(DigestKind.ShoppingEvening) ? await shopping.ListNeededAsync(cancellationToken) : null,
            Week: week is null ? null : await expenses.GetPeriodAsync(week.From, week.To, cancellationToken),
            WeekMonth: week is null ? null : await expenses.GetMonthAsync(week.To.Year, week.To.Month, cancellationToken),
            Month: month is null ? null : await expenses.GetMonthAsync(month.From.Year, month.From.Month, cancellationToken));
    }
}

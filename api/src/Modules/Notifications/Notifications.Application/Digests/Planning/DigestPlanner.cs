using System.Globalization;
using Notifications.Application.Digests.Texts;
using Notifications.Application.Sending;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Schedule;
using Todos.Contracts;

namespace Notifications.Application.Digests.Planning;

/// <summary>
/// Turns the due slots and what was read into messages for the linked chats. Pure: the tick decides which of them
/// are new by their keys; a digest with nothing to say (no tasks, an empty list) gives no message.
/// </summary>
public static class DigestPlanner
{
    public static IReadOnlyList<PlannedMessage> Plan(
        IReadOnlyList<DueSlot> slots,
        IReadOnlyDictionary<Audience, long> chats,
        DigestInputs inputs,
        DateOnly today,
        NotificationsSettings site)
    {
        var messages = new List<PlannedMessage>();
        foreach (var slot in slots)
        {
            if (slot.Kind == DigestKind.GiftReminder)
            {
                messages.AddRange(GiftReminders(inputs, chats, today));
                continue;
            }

            foreach (var audience in DigestRoutes.For(slot.Kind, chats))
            {
                if (Render(slot, audience, inputs, today, site) is { } message)
                {
                    messages.Add(new PlannedMessage(
                        Delivery.KeyOf(slot.Kind, slot.Period, audience),
                        slot.Kind,
                        audience,
                        chats[audience],
                        message.Text,
                        message.Buttons,
                        PageOf(slot.Kind)));
                }
            }
        }

        return messages;
    }

    private static (string Text, IReadOnlyList<TelegramButton> Buttons)? Render(
        DueSlot slot, Audience audience, DigestInputs inputs, DateOnly today, NotificationsSettings site) =>
        slot.Kind switch
        {
            DigestKind.TodosMorning when inputs.Todos is { } todos =>
                TodoTexts.Morning(Assignee(audience), TextFormat.Address(audience.ToString()), todos, today),
            DigestKind.TodosCommon when inputs.Todos is { } todos =>
                Plain(TodoTexts.Common(todos, today, site.Page("todos"))),
            DigestKind.ShoppingEvening when inputs.Shopping is { } items =>
                Plain(ShoppingText.For(items, site.Page("shopping"))),
            DigestKind.ExpensesWeek when inputs is { Week: { } week, WeekMonth: { } month } =>
                Plain(ExpensesTexts.Week(week, month, site.Page("expenses/forecast"))),
            DigestKind.ExpensesMonth when inputs.Month is { } month =>
                Plain(ExpensesTexts.Month(month, site.Page("expenses/forecast"))),
            _ => null,
        };

    private static IEnumerable<PlannedMessage> GiftReminders(DigestInputs inputs, IReadOnlyDictionary<Audience, long> chats, DateOnly today) =>
        from gift in inputs.Gifts ?? []
        from audience in DigestRoutes.Givers(gift.Giver, chats)
        let period = $"{gift.GiftId:N}:{gift.BuyBy.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
        select new PlannedMessage(
            Delivery.KeyOf(DigestKind.GiftReminder, period, audience),
            DigestKind.GiftReminder,
            audience,
            chats[audience],
            GiftReminderText.For(gift, today),
            [],
            $"/gifts?gift={gift.GiftId}");

    /// <summary>The site page a digest is about (spec notifications/002, R-107).</summary>
    public static string PageOf(DigestKind kind) => kind switch
    {
        DigestKind.TodosMorning or DigestKind.TodosCommon => "/todos",
        DigestKind.ShoppingEvening => "/shopping",
        DigestKind.ExpensesWeek or DigestKind.ExpensesMonth => "/expenses/forecast",
        _ => "/gifts",
    };

    private static (string Text, IReadOnlyList<TelegramButton> Buttons)? Plain(string? text) =>
        text is null ? null : (text, []);

    public static TodoAssignee Assignee(Audience person) =>
        person == Audience.Maria ? TodoAssignee.Maria : TodoAssignee.Vitaliy;
}

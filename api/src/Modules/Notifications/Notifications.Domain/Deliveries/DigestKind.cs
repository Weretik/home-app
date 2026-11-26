namespace Notifications.Domain.Deliveries;

/// <summary>The kinds of scheduled messages (spec notifications/001, R-008…R-013).</summary>
public enum DigestKind
{
    TodosMorning,
    TodosCommon,
    GiftReminder,
    ShoppingEvening,
    ExpensesWeek,
    ExpensesMonth,
}

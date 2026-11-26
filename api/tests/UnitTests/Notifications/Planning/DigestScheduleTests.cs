using Notifications.Domain.Deliveries;
using Notifications.Domain.Schedule;

namespace UnitTests.Notifications.Planning;

public sealed class DigestScheduleTests
{
    private static IReadOnlyList<DigestKind> Due(int year, int month, int day, int hour, int minute = 0) =>
        [.. DigestSchedule.DueAt(new DateTime(year, month, day, hour, minute, 0)).Select(s => s.Kind)];

    [Fact]
    public void The_Morning_Window_Runs_From8_Until12()
    {
        Assert.Empty(Due(2026, 12, 2, 7, 59));
        Assert.Equal([DigestKind.TodosMorning, DigestKind.TodosCommon, DigestKind.GiftReminder], Due(2026, 12, 2, 8));
        Assert.Equal([DigestKind.TodosMorning, DigestKind.TodosCommon, DigestKind.GiftReminder], Due(2026, 12, 2, 11, 59));
        Assert.Empty(Due(2026, 12, 2, 12));
    }

    [Fact]
    public void The_ShoppingList_IsDue_From18_Until22()
    {
        Assert.Empty(Due(2026, 12, 2, 17, 45));
        Assert.Equal([DigestKind.ShoppingEvening], Due(2026, 12, 2, 18));
        Assert.Empty(Due(2026, 12, 2, 22));
    }

    [Fact]
    public void The_Week_IsDue_OnSundayEvening_WithTheMondayAsItsPeriod()
    {
        var slot = Assert.Single(DigestSchedule.DueAt(new DateTime(2026, 12, 6, 20, 15, 0)), s => s.Kind == DigestKind.ExpensesWeek);

        Assert.Equal("2026-11-30", slot.Period);
        Assert.Equal(new DateOnly(2026, 11, 30), slot.From);
        Assert.Equal(new DateOnly(2026, 12, 6), slot.To);
        Assert.DoesNotContain(DigestKind.ExpensesWeek, Due(2026, 12, 6, 19, 45));
        Assert.DoesNotContain(DigestKind.ExpensesWeek, Due(2026, 12, 5, 21));
    }

    [Fact]
    public void The_Month_IsDue_OnTheLastDayEvening_AndCaughtUp_OnTheFirstMorning()
    {
        var lastDay = Assert.Single(DigestSchedule.DueAt(new DateTime(2026, 11, 30, 20, 0, 0)), s => s.Kind == DigestKind.ExpensesMonth);
        var catchUp = Assert.Single(DigestSchedule.DueAt(new DateTime(2026, 12, 1, 9, 0, 0)), s => s.Kind == DigestKind.ExpensesMonth);

        Assert.Equal("2026-11", lastDay.Period);
        Assert.Equal(lastDay, catchUp);
        Assert.Equal(new DateOnly(2026, 11, 30), lastDay.To);
        Assert.DoesNotContain(DigestKind.ExpensesMonth, Due(2026, 12, 1, 12));
        Assert.DoesNotContain(DigestKind.ExpensesMonth, Due(2026, 11, 29, 21));
    }

    [Fact]
    public void A_Delivery_Key_IsKindPeriodAndAudience()
    {
        Assert.Equal("TodosMorning:2026-12-02:Maria", Delivery.KeyOf(DigestKind.TodosMorning, "2026-12-02", global::Notifications.Domain.Chats.Audience.Maria));
        Assert.Equal(6, DigestSchedule.Windows.Count);
    }
}

using Gifts.Contracts;
using Notifications.Application;
using Notifications.Application.Digests.Planning;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Schedule;
using Shopping.Contracts;
using Todos.Contracts;

namespace UnitTests.Notifications.Planning;

public sealed class DigestPlannerTests
{
    private static readonly DateOnly Today = new(2026, 12, 2);
    private static readonly NotificationsSettings Site = new("https://web.example/");
    private static readonly Dictionary<Audience, long> Chats = new() { [Audience.Vitaliy] = 1, [Audience.Maria] = 2, [Audience.Family] = -100 };

    private static IReadOnlyList<DueSlot> Morning => DigestSchedule.DueAt(new DateTime(2026, 12, 2, 8, 0, 0));

    [Fact]
    public void The_Morning_Plans_PersonalMessages_ForThoseWithTasks_AndTheCommonOne()
    {
        var inputs = new DigestInputs(
            Todos: [new OpenTodo(Guid.NewGuid(), "Позвонить маме", TodoAssignee.Maria, Today, false)],
            Gifts: []);

        var planned = DigestPlanner.Plan(Morning, Chats, inputs, Today, Site);

        Assert.Equal(
            ["TodosMorning:2026-12-02:Maria", "TodosCommon:2026-12-02:Family"],
            planned.Select(m => m.Key));
        Assert.Equal(2, planned[0].ChatId);
        Assert.Single(planned[0].Buttons);
        Assert.Contains("https://web.example/todos", planned[1].Text);
    }

    [Fact]
    public void Nothing_ToSay_PlansNothing()
    {
        var evening = DigestSchedule.DueAt(new DateTime(2026, 12, 2, 18, 0, 0));

        Assert.Empty(DigestPlanner.Plan(Morning, Chats, new DigestInputs(Todos: [], Gifts: []), Today, Site));
        Assert.Empty(DigestPlanner.Plan(evening, Chats, new DigestInputs(Shopping: []), Today, Site));
        Assert.Single(DigestPlanner.Plan(evening, Chats, new DigestInputs(Shopping: [new NeededItem("Молоко", null, false, "Молочное", "🥛")]), Today, Site));
    }

    [Fact]
    public void A_Gift_Reminder_IsKeyedByGiftAndDate_SoItGoesOnce_PerDate()
    {
        var gift = new GiftPurchase(Guid.Parse("0193b6a0-0000-7000-8000-0000000000aa"), "Кольцо", GiftParty.Maria, GiftParty.Vitaliy, new DateOnly(2026, 12, 20), "https://web.example/gifts?gift=1");

        var reminder = Assert.Single(DigestPlanner.Plan(Morning, Chats, new DigestInputs(Todos: [], Gifts: [gift]), Today, Site));

        Assert.Equal("GiftReminder:0193b6a00000700080000000000000aa:2026-12-20:Vitaliy", reminder.Key);
        Assert.Equal(DigestKind.GiftReminder, reminder.Kind);
    }

    [Fact]
    public void For_Push_WithoutAFamilyEntry_CommonMessagesGoToEachPersonWithADevice()
    {
        var pushPeople = new Dictionary<Audience, long> { [Audience.Vitaliy] = 0, [Audience.Maria] = 0 };
        var inputs = new DigestInputs(
            Todos: [new OpenTodo(Guid.NewGuid(), "Позвонить маме", TodoAssignee.Maria, Today, false)],
            Gifts: []);

        var planned = DigestPlanner.Plan(Morning, pushPeople, inputs, Today, Site);

        Assert.Equal(
            ["TodosMorning:2026-12-02:Maria", "TodosCommon:2026-12-02:Maria"],
            planned.Select(m => m.Key));
        Assert.All(planned, m => Assert.Equal("/todos", m.Page));
    }

    [Fact]
    public void A_Gift_Reminder_ForPush_OpensTheGift_AndSkipsAGiverWithoutADevice()
    {
        var id = Guid.Parse("0193b6a0-0000-7000-8000-0000000000aa");
        var gift = new GiftPurchase(id, "Кольцо", GiftParty.Maria, GiftParty.Vitaliy, new DateOnly(2026, 12, 20), "https://web.example/gifts?gift=1");
        var inputs = new DigestInputs(Todos: [], Gifts: [gift]);

        var reminder = Assert.Single(DigestPlanner.Plan(Morning, new Dictionary<Audience, long> { [Audience.Vitaliy] = 0 }, inputs, Today, Site));
        var none = DigestPlanner.Plan(Morning, new Dictionary<Audience, long> { [Audience.Maria] = 0 }, inputs, Today, Site);

        Assert.Equal($"/gifts?gift={id}", reminder.Page);
        Assert.Empty(none);
    }

    [Fact]
    public void A_Person_MapsToTheirAssignee()
    {
        Assert.Equal(TodoAssignee.Maria, DigestPlanner.Assignee(Audience.Maria));
        Assert.Equal(TodoAssignee.Vitaliy, DigestPlanner.Assignee(Audience.Vitaliy));
    }
}

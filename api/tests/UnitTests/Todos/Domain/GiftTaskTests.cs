using Gifts.Contracts;
using Todos.Application.Integrations.Gifts;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace UnitTests.Todos.Domain;

public sealed class GiftTaskTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_Gift_Task_NamesTheGift_AndForWhom_AndFitsTheTitle()
    {
        Assert.Equal("🎁 Купити подарунок: Кольцо (для Маші)", GiftTask.Title(" Кольцо ", GiftParty.Maria));
        Assert.Equal("🎁 Купити подарунок: Часы (для Віталія)", GiftTask.Title("Часы", GiftParty.Vitaliy));
        Assert.Equal("🎁 Купити подарунок: Кофемашина", GiftTask.Title("Кофемашина", GiftParty.Together));

        var longTitle = GiftTask.Title(new string('a', 300), GiftParty.Maria);
        Assert.Equal(TodoTask.MaxTitleLength, longTitle.Length);
        Assert.EndsWith("… (для Маші)", longTitle);
    }

    [Fact]
    public void The_Giver_BecomesTheAssignee()
    {
        Assert.Equal(Assignee.Maria, GiftTask.AssigneeOf(GiftParty.Maria));
        Assert.Equal(Assignee.Vitaliy, GiftTask.AssigneeOf(GiftParty.Vitaliy));
        Assert.Equal(Assignee.Shared, GiftTask.AssigneeOf(GiftParty.Together));
        Assert.Equal("Подарунок на сайті: https://web.example/gifts?gift=1", GiftTask.Notes("https://web.example/gifts?gift=1"));
    }

    [Fact]
    public void A_Source_IsLinkedOnce_AndMustBeValid()
    {
        var task = TodoTask.Create("Купить", null, Assignee.Vitaliy, null, TodoStatus.Todo, TodoPriority.High, null, null, [], Now).Value;
        var gift = Guid.NewGuid();

        Assert.Equal("todos.source_invalid", task.LinkSource("gift", Guid.Empty).Error!.Code);
        Assert.Equal("todos.source_invalid", task.LinkSource(" ", gift).Error!.Code);
        Assert.True(task.LinkSource("gift", gift).IsSuccess);
        Assert.Equal("todos.source_already_set", task.LinkSource("gift", Guid.NewGuid()).Error!.Code);
        Assert.Equal(("gift", gift), (task.SourceKind, task.SourceId!.Value));
    }
}

using Gifts.Contracts;
using Notifications.Application.Digests.Planning;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace UnitTests.Notifications.Planning;

public sealed class DigestRoutesTests
{
    private static readonly Dictionary<Audience, long> People = new() { [Audience.Vitaliy] = 1, [Audience.Maria] = 2 };
    private static readonly Dictionary<Audience, long> All = new(People) { [Audience.Family] = -100 };

    [Fact]
    public void Personal_Messages_GoOnly_ToLinkedPeople()
    {
        Assert.Equal([Audience.Vitaliy, Audience.Maria], DigestRoutes.For(DigestKind.TodosMorning, All));
        Assert.Equal([Audience.Maria], DigestRoutes.For(DigestKind.TodosMorning, new Dictionary<Audience, long> { [Audience.Maria] = 2, [Audience.Family] = -1 }));
    }

    [Fact]
    public void Common_Messages_GoToTheGroup_OrToBothPeople_WithoutOne()
    {
        Assert.Equal([Audience.Family], DigestRoutes.For(DigestKind.ShoppingEvening, All));
        Assert.Equal([Audience.Vitaliy, Audience.Maria], DigestRoutes.For(DigestKind.ExpensesWeek, People));
        Assert.Empty(DigestRoutes.Common(new Dictionary<Audience, long>()));
    }

    [Fact]
    public void A_Gift_Reminder_GoesToTheGiver_ElseToTheGroup()
    {
        Assert.Equal([Audience.Maria], DigestRoutes.Givers(GiftParty.Maria, All));
        Assert.Equal([Audience.Vitaliy, Audience.Maria], DigestRoutes.Givers(GiftParty.Together, All));
        Assert.Equal([Audience.Family], DigestRoutes.Givers(GiftParty.Maria, new Dictionary<Audience, long> { [Audience.Vitaliy] = 1, [Audience.Family] = -1 }));
        Assert.Equal([Audience.Vitaliy, Audience.Family], DigestRoutes.Givers(GiftParty.Together, new Dictionary<Audience, long> { [Audience.Vitaliy] = 1, [Audience.Family] = -1 }));
        Assert.Empty(DigestRoutes.Givers(GiftParty.Maria, new Dictionary<Audience, long> { [Audience.Vitaliy] = 1 }));
    }
}

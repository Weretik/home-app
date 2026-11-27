using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Push;

namespace UnitTests.Notifications.Push;

public sealed class PushSubscriptionTests
{
    private static readonly DateTimeOffset Then = new(2026, 11, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Saving_Again_RefreshesKeysOwnerLabelAndLastSeen_ButKeepsTheRow()
    {
        var subscription = PushSubscription.Save(Audience.Vitaliy, "https://push.example/1", "k1", "a1", "  Android · Chrome  ", Then);

        subscription.Refresh(Audience.Maria, "k2", "a2", new string('x', 200), Then.AddDays(1));

        Assert.Equal(Audience.Maria, subscription.Person);
        Assert.Equal(("k2", "a2"), (subscription.P256dh, subscription.Auth));
        Assert.Equal(PushSubscription.MaxDeviceLength, subscription.Device!.Length);
        Assert.Equal(Then, subscription.CreatedAtUtc);
        Assert.Equal(Then.AddDays(1), subscription.LastSeenAtUtc);
    }

    [Fact]
    public void An_Empty_Label_IsNone() =>
        Assert.Null(PushSubscription.Save(Audience.Maria, "https://push.example/2", "k", "a", "   ", Then).Device);

    [Fact]
    public void Push_Keys_StandBesideTheTelegramKeys_WhichStayAsTheyWere()
    {
        var telegram = Delivery.KeyOf(DigestKind.ShoppingEvening, "2026-11-27", Audience.Maria);

        Assert.Equal("ShoppingEvening:2026-11-27:Maria", Delivery.KeyOf(DeliveryChannel.Telegram, telegram));
        Assert.Equal("push:ShoppingEvening:2026-11-27:Maria", Delivery.KeyOf(DeliveryChannel.Push, telegram));
    }
}

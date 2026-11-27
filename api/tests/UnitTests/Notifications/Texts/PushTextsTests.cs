using Notifications.Application.Digests.Planning;
using Notifications.Application.Digests.Texts;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace UnitTests.Notifications.Texts;

/// <summary>Push texts from the Telegram ones (spec notifications/002, R-107).</summary>
public sealed class PushTextsTests
{
    [Fact]
    public void Html_BecomesPlainText_WithoutLinks()
    {
        var html = "🛒 <b>Список покупок</b>\n\n\n🥛 Молочне: молоко &amp; кефір\n" + TextFormat.Link("https://web.example/shopping", "Відкрити список");

        Assert.Equal("🛒 Список покупок\n\n🥛 Молочне: молоко & кефір", PushTexts.Plain(html));
    }

    [Fact]
    public void A_Long_Text_IsCut()
    {
        var text = PushTexts.Plain(new string('я', 3000));

        Assert.Equal(PushTexts.MaxBodyLength, text.Length);
        Assert.EndsWith("…", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_Payload_HasTheTitleOfItsKind_ThePage_AndATag()
    {
        var shopping = new PlannedMessage("k", DigestKind.ShoppingEvening, Audience.Maria, 0, "<b>x</b>", [], "/shopping");
        var gift = shopping with { Kind = DigestKind.GiftReminder, Page = "/gifts?gift=1" };

        var payload = PushTexts.For(shopping);

        Assert.Equal(("Список покупок", "x", "/shopping", "ShoppingEvening"), (payload.Title, payload.Body, payload.Url, payload.Tag));
        Assert.Equal("gift:/gifts?gift=1", PushTexts.For(gift).Tag);
        Assert.All(Enum.GetValues<DigestKind>(), kind => Assert.False(string.IsNullOrWhiteSpace(PushTexts.Title(kind))));
    }
}

using Gifts.Contracts;
using Notifications.Application.Digests.Texts;
using Shopping.Contracts;

namespace UnitTests.Notifications.Texts;

public sealed class ListTextsTests
{
    [Fact]
    public void The_Shopping_List_IsGroupedByDepartment()
    {
        var text = ShoppingText.For(
            [
                new NeededItem("Молоко", "2 л", true, "Молочное", "🥛"),
                new NeededItem("Сыр", null, false, "Молочное", "🥛"),
                new NeededItem("Хлеб", null, false, "Выпечка", "🍞"),
            ],
            "https://web.example/shopping")!;

        Assert.Equal(
            "🛒 <b>У списку покупок: 3</b>\n\n🥛 <b>Молочное</b>\n• ❗ Молоко — 2 л\n• Сыр\n\n🍞 <b>Выпечка</b>\n• Хлеб\n\n"
            + "<a href=\"https://web.example/shopping\">Відкрити список</a>",
            text);
        Assert.Null(ShoppingText.For([], "u"));
    }

    [Fact]
    public void A_Gift_Reminder_SaysForWhom_ByWhen_AndLinksTheGift()
    {
        var gift = new GiftPurchase(Guid.NewGuid(), "Кольцо", GiftParty.Maria, GiftParty.Vitaliy, new DateOnly(2026, 12, 20), "https://web.example/gifts?gift=1");

        Assert.Equal(
            "🎁 <b>Час купити подарунок</b>\nКольцо (для: Маша)\nКупити до 20.12 — через 14 дн.\n"
            + "<a href=\"https://web.example/gifts?gift=1\">Відкрити подарунок</a>",
            GiftReminderText.For(gift, new DateOnly(2026, 12, 6)));
        Assert.Contains("— завтра", GiftReminderText.For(gift, new DateOnly(2026, 12, 19)));
        Assert.Contains("— сьогодні", GiftReminderText.For(gift, new DateOnly(2026, 12, 20)));
    }
}

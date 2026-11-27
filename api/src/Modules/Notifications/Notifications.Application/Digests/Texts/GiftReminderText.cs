using Gifts.Contracts;

namespace Notifications.Application.Digests.Texts;

/// <summary>The reminder to buy a gift (spec notifications/001, R-010).</summary>
public static class GiftReminderText
{
    public static string For(GiftPurchase gift, DateOnly today)
    {
        var days = gift.BuyBy.DayNumber - today.DayNumber;
        var when = days switch
        {
            0 => "сьогодні",
            1 => "завтра",
            _ => $"через {days} дн.",
        };
        var forWhom = gift.Recipient == GiftParty.Together ? "для дому, разом" : $"для: {TextFormat.Person(gift.Recipient.ToString())}";
        return $"🎁 <b>Час купити подарунок</b>\n{TextFormat.Escape(gift.Name)} ({forWhom})\n"
            + $"Купити до {TextFormat.Day(gift.BuyBy)} — {when}\n"
            + TextFormat.Link(gift.Link, "Відкрити подарунок");
    }
}

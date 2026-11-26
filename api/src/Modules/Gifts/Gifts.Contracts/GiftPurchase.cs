namespace Gifts.Contracts;

/// <summary>A wanted gift with a giver and a buy-by date, for the reminders of the Telegram bot.</summary>
/// <param name="Link">The gift on the site, <c>&lt;site&gt;/gifts?gift=&lt;id&gt;</c>.</param>
public sealed record GiftPurchase(
    Guid GiftId,
    string Name,
    GiftParty Recipient,
    GiftParty Giver,
    DateOnly BuyBy,
    string Link);

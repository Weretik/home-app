namespace Gifts.Contracts;

/// <summary>
/// Integration event: a wanted gift has a giver and a buy-by date, or one of its name, recipient, giver or date
/// changed. Todos keeps one task per gift from it (spec gifts/002, ADR 0035).
/// </summary>
/// <param name="Link">The gift on the site, <c>&lt;site&gt;/gifts?gift=&lt;id&gt;</c>.</param>
public sealed record GiftPurchasePlanned(
    Guid GiftId,
    string GiftName,
    GiftParty Recipient,
    GiftParty Giver,
    DateOnly BuyBy,
    string Link);

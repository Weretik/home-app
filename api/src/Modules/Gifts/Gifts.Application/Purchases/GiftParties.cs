using Gifts.Contracts;
using Gifts.Domain.People;

namespace Gifts.Application.Purchases;

/// <summary>The module's own <see cref="Recipient"/> as the contract's <see cref="GiftParty"/>.</summary>
internal static class GiftParties
{
    public static GiftParty ToParty(this Recipient person) => person switch
    {
        Recipient.Maria => GiftParty.Maria,
        Recipient.Vitaliy => GiftParty.Vitaliy,
        _ => GiftParty.Together,
    };
}

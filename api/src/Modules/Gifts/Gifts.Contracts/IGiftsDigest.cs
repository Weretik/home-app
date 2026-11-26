namespace Gifts.Contracts;

/// <summary>What the Notifications module may read from Gifts; implemented by the Gifts module.</summary>
public interface IGiftsDigest
{
    /// <summary>Wanted gifts with a giver whose buy-by date is between <paramref name="from"/> and <paramref name="to"/>, earliest first.</summary>
    Task<IReadOnlyList<GiftPurchase>> ListPurchasesDueAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
}

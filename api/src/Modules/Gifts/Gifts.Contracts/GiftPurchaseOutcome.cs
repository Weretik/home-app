namespace Gifts.Contracts;

/// <summary>Why a gift stopped being planned: only <see cref="Gifted"/> completes its task, the rest cancel it.</summary>
public enum GiftPurchaseOutcome
{
    Gifted,
    Dropped,
    Unplanned,
    Deleted,
}

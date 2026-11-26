namespace Gifts.Contracts;

/// <summary>Integration event: a planned gift is no longer planned, for <see cref="Outcome"/>.</summary>
public sealed record GiftPurchaseClosed(Guid GiftId, GiftPurchaseOutcome Outcome);

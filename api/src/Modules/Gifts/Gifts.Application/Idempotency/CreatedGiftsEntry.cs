namespace Gifts.Application.Idempotency;

/// <summary>The id of a created gift, category or detail.</summary>
public sealed record CreatedGiftsEntry(Guid Id);

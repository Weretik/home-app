using Gifts.Domain.Wishlist;

namespace Gifts.Api.Endpoints.Status;

/// <summary>The new status; <see cref="GiftedOn"/> only counts for <c>Gifted</c> (today in Kyiv when missing).</summary>
internal sealed record GiftStatusRequest(GiftStatus Status, DateOnly? GiftedOn);

using Gifts.Domain.People;

namespace Gifts.Api.Endpoints.Details;

/// <summary>The fields of a detail, for adding or changing one.</summary>
internal sealed record GiftDetailRequest(Recipient Recipient, string Label, string Value);

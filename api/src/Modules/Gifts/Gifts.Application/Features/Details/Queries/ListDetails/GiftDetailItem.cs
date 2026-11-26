using Gifts.Domain.People;

namespace Gifts.Application.Features.Details.Queries.ListDetails;

/// <summary>A detail («нюанс») as settings and the home summary show it.</summary>
public sealed record GiftDetailItem(Guid Id, uint Version, Recipient Recipient, string Label, string Value, int SortOrder);

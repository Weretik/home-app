using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Items.Queries.ListGifts;

/// <summary>
/// The active list (<see cref="Archived"/> false: wanted gifts) or the archive (true: gifted and dropped ones), whole,
/// without pages: the family has a few hundred gifts at most.
/// </summary>
public sealed record ListGiftsQuery(bool Archived) : IQuery<Result<IReadOnlyList<GiftItem>>>;

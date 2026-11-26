using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Details.Queries.ListDetails;

/// <summary>All details: Маша's, then Виталий's, then the shared ones, each in its own order.</summary>
public sealed record ListDetailsQuery : IQuery<Result<IReadOnlyList<GiftDetailItem>>>;

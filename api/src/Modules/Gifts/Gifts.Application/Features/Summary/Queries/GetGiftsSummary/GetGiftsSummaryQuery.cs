using BuildingBlocks.Domain.Results;
using Gifts.Application.Features.Summary.Report;
using Mediator;

namespace Gifts.Application.Features.Summary.Queries.GetGiftsSummary;

/// <summary>The wishlist's figures for the home page.</summary>
public sealed record GetGiftsSummaryQuery : IQuery<Result<GiftsSummary>>;

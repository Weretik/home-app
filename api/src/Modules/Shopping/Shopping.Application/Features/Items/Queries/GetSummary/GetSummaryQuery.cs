using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>The list at a glance for the home page.</summary>
public sealed record GetSummaryQuery : IQuery<Result<ShoppingSummary>>;

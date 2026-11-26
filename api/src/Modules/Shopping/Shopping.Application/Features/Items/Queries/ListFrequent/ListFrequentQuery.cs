using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Queries.ListFrequent;

/// <summary>«⭐ Частые»: the 20 names bought most often in the window, not in the list now.</summary>
public sealed record ListFrequentQuery : IQuery<Result<IReadOnlyList<FrequentItem>>>;

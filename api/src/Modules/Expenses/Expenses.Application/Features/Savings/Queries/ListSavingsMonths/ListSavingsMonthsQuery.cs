using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMonths;

/// <summary>Every month that has savings movements, with its sums and closing balance.</summary>
public sealed record ListSavingsMonthsQuery : IQuery<Result<SavingsOverview>>;

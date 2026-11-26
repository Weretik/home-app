using BuildingBlocks.Domain.Results;
using Expenses.Application.Features.Summary.Report;
using Mediator;

namespace Expenses.Application.Features.Summary.Queries.GetMonthSummary;

/// <summary>The summary of one calendar month.</summary>
public sealed record GetMonthSummaryQuery(int Year, int Month) : IQuery<Result<MonthSummary>>;

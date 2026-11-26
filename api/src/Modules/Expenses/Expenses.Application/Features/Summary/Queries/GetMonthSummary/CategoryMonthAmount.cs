namespace Expenses.Application.Features.Summary.Queries.GetMonthSummary;

/// <summary>A sum for one category in one month (the first day of the month), as read by one grouped query.</summary>
internal sealed record CategoryMonthAmount(Guid CategoryId, DateOnly Month, decimal Amount);

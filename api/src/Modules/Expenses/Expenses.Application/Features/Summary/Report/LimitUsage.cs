using Expenses.Domain.Budget;

namespace Expenses.Application.Features.Summary.Report;

/// <summary>
/// A spending limit against what was spent in its month or quarter (the quarter that holds the summary's month).
/// </summary>
/// <param name="From">The first day of the limit's period.</param>
/// <param name="To">The last day of the limit's period.</param>
/// <param name="Left">The limit minus what was spent; negative when it is overspent.</param>
/// <param name="Percent">Spent as a share of the limit, in percent, rounded.</param>
/// <param name="PerDay">What may still be spent per day until the period ends, or null when the period is not running now.</param>
public sealed record LimitUsage(
    Guid Id,
    string Name,
    LimitPeriod Period,
    DateOnly From,
    DateOnly To,
    decimal Amount,
    decimal Spent,
    decimal Left,
    int Percent,
    decimal? PerDay);

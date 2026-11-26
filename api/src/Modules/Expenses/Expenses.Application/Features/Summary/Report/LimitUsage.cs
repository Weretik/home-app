using Expenses.Domain.Budget;

namespace Expenses.Application.Features.Summary.Report;

/// <summary>
/// A spending limit against what was spent in its month or quarter (the quarter that holds the summary's month),
/// with the overspend carried in from earlier periods like the old «Бюджет» sheet. Expenses marked «Не в лимиты»
/// never count here.
/// </summary>
/// <param name="From">The first day of the limit's period.</param>
/// <param name="To">The last day of the limit's period.</param>
/// <param name="Spent">What was spent on the limit's categories in the period.</param>
/// <param name="CarriedIn">Overspend of earlier periods this period has to cover; 0 when there is none.</param>
/// <param name="Left">The limit minus what was spent minus <see cref="CarriedIn"/>; negative when it is overspent.</param>
/// <param name="Percent">Spent plus <see cref="CarriedIn"/> as a share of the limit, in percent, rounded.</param>
/// <param name="PerDay">What may still be spent per day until the period ends, or null when the period is not running now.</param>
/// <param name="CarriesOut">What this period passes on to the next one if nothing more is spent.</param>
/// <param name="Planned">Planned expenses of the limit's categories in the months of the period.</param>
public sealed record LimitUsage(
    Guid Id,
    string Name,
    LimitPeriod Period,
    DateOnly From,
    DateOnly To,
    decimal Amount,
    decimal Spent,
    decimal CarriedIn,
    decimal Left,
    int Percent,
    decimal? PerDay,
    decimal CarriesOut,
    decimal Planned);

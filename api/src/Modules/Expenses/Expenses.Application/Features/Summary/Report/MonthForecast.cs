namespace Expenses.Application.Features.Summary.Report;

/// <summary>
/// Where the month is heading once the planned expenses («Прогноз») are spent too.
/// </summary>
/// <param name="Planned">The planned expenses of the month.</param>
/// <param name="ExpectedExpenses">The month's expenses plus <see cref="Planned"/>.</param>
/// <param name="ExpectedResult">Incomes minus <see cref="ExpectedExpenses"/> minus what went into the savings jar.</param>
/// <param name="ExpectedTotalLeft">
/// The monthly limit minus the expenses it counts, minus <see cref="Planned"/>, minus what was carried in from earlier
/// months; 0 when there is no monthly limit.
/// </param>
public sealed record MonthForecast(
    decimal Planned,
    decimal ExpectedExpenses,
    decimal ExpectedResult,
    decimal ExpectedTotalLeft);

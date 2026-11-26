namespace Expenses.Application.Features.Summary.Report;

/// <summary>
/// One month at a glance, like the old «Сводка» sheet without its plan block: incomes and expenses with who they
/// belong to, the month's result after the savings jar, the jar itself, the spending limits with their carry-over,
/// the top categories and the forecast with the planned expenses.
/// </summary>
/// <param name="Expenses">Every expense of the month, those marked «Не в лимиты» included.</param>
/// <param name="OutsideLimitsCount">How many expenses of the month are marked «Не в лимиты».</param>
/// <param name="OutsideLimitsAmount">What those expenses add up to; they are part of <see cref="Expenses"/>.</param>
/// <param name="MonthlyLimit">The limit on all expenses of a month; 0 means none.</param>
/// <param name="CarriedIn">
/// Overspend of the monthly limit carried in from earlier months; 0 when there is no monthly limit.
/// </param>
/// <param name="CarriesOut">
/// What the month passes on to the next one: the expenses the limit counts plus <see cref="CarriedIn"/> over the
/// limit; 0 when there is no monthly limit.
/// </param>
/// <param name="DaysLeft">Days left in the month counting today, or null when the month is not the current one.</param>
/// <param name="Result">Incomes minus expenses minus what was put into the jar (plus what was taken out).</param>
/// <param name="SavingsNet">Put into the jar minus taken out during the month.</param>
/// <param name="SavingsBalance">What the jar held at the end of the month.</param>
/// <param name="SavingsGoal">How much the jar should hold; 0 means no goal.</param>
public sealed record MonthSummary(
    int Year,
    int Month,
    decimal Incomes,
    IReadOnlyList<PersonAmount> IncomesByPerson,
    decimal Expenses,
    IReadOnlyList<PayerAmount> ExpensesByPayer,
    int OutsideLimitsCount,
    decimal OutsideLimitsAmount,
    decimal MonthlyLimit,
    decimal CarriedIn,
    decimal CarriesOut,
    int? DaysLeft,
    decimal Result,
    decimal SavingsNet,
    decimal SavingsBalance,
    decimal SavingsGoal,
    IReadOnlyList<LimitUsage> Limits,
    IReadOnlyList<CategoryAmount> TopCategories,
    MonthForecast Forecast);

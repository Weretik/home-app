using Expenses.Domain.Budget;

namespace Expenses.Api.Endpoints.Budget.Limits;

/// <summary>
/// A spending limit: its name, whether it is per month or per quarter, the amount and the categories it counts. On a
/// change, <see cref="AmountFrom"/> is a day of the first month (or quarter) the amount applies to (left out: the
/// current one); earlier periods keep their amount. A new limit ignores it: its amount applies to every period.
/// </summary>
internal sealed record SpendingLimitRequest(
    string Name, LimitPeriod Period, decimal Amount, IReadOnlyList<Guid> CategoryIds, DateOnly? AmountFrom = null);

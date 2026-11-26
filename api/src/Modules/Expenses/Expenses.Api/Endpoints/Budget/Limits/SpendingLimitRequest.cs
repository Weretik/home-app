using Expenses.Domain.Budget;

namespace Expenses.Api.Endpoints.Budget.Limits;

/// <summary>A spending limit: its name, whether it is per month or per quarter, the amount and the categories it counts.</summary>
internal sealed record SpendingLimitRequest(string Name, LimitPeriod Period, decimal Amount, IReadOnlyList<Guid> CategoryIds);

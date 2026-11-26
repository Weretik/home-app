using Expenses.Domain.Budget;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>One spending limit with the categories it counts.</summary>
public sealed record SpendingLimitItem(
    Guid Id,
    uint Version,
    string Name,
    LimitPeriod Period,
    decimal Amount,
    IReadOnlyList<Guid> CategoryIds);

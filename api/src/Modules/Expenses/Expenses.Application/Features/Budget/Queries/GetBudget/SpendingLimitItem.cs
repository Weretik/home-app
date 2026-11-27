using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>
/// One spending limit with the categories it counts: the amount of the current period, and its amounts as steps by
/// the period they start in, oldest first.
/// </summary>
public sealed record SpendingLimitItem(
    Guid Id,
    uint Version,
    string Name,
    LimitPeriod Period,
    decimal Amount,
    IReadOnlyList<AmountStep> Amounts,
    IReadOnlyList<Guid> CategoryIds);

using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Planned.Queries.ListPlannedExpenses;

/// <summary>One planned expense of the month, with its category name.</summary>
public sealed record PlannedExpenseItem(
    Guid Id,
    uint Version,
    int Year,
    int Month,
    string Name,
    Guid CategoryId,
    string CategoryName,
    Payer Payer,
    decimal Amount);

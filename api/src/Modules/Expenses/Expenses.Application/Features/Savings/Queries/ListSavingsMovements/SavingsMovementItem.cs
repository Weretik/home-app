using Expenses.Domain.Parties;
using Expenses.Domain.Savings;

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMovements;

/// <summary>One movement of the savings jar; <see cref="Amount"/> is signed (plus in, minus out).</summary>
public sealed record SavingsMovementItem(
    Guid Id,
    uint Version,
    DateOnly Date,
    Person Person,
    decimal Amount,
    SavingsDirection Direction,
    string? Comment);

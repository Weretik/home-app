using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

/// <summary>One expense row of the month list, with its category name.</summary>
public sealed record ExpenseItem(
    Guid Id,
    uint Version,
    DateOnly Date,
    Payer Payer,
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string? Comment);

using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Imports.SheetFormat.Rows;

/// <summary>A «Расходы» row after reading: typed values, the category still by name, and its sheet row number.</summary>
internal sealed record SheetExpense(
    int Row,
    DateOnly Date,
    Payer Payer,
    string CategoryName,
    decimal Amount,
    PaymentMethod PaymentMethod,
    string? Comment);

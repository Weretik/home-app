using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Api.Endpoints.Expenses;

/// <summary>The fields of an expense, for adding or changing one.</summary>
internal sealed record ExpenseRequest(DateOnly Date, Payer Payer, Guid CategoryId, decimal Amount, PaymentMethod PaymentMethod, string? Comment);

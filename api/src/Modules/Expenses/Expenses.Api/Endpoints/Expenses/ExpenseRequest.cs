using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Api.Endpoints.Expenses;

/// <summary>
/// The fields of an expense, for adding or changing one. <see cref="OutsideLimits"/> («Не в лимиты», false when left
/// out) keeps the expense in the totals but out of every limit and of the carry-over.
/// </summary>
internal sealed record ExpenseRequest(
    DateOnly Date, Payer Payer, Guid CategoryId, decimal Amount, PaymentMethod PaymentMethod, string? Comment, bool OutsideLimits = false);

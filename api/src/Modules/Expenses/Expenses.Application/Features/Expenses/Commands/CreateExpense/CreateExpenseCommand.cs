using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Expenses.Commands.CreateExpense;

/// <summary>
/// Adds an expense. A repeat with the same Idempotency-Key and data returns the first id. <see cref="OutsideLimits"/>
/// marks a one-off spend that counts in the totals but in no limit.
/// </summary>
public sealed record CreateExpenseCommand(string IdempotencyKey, DateOnly Date, Payer Payer, Guid CategoryId, decimal Amount, PaymentMethod PaymentMethod, string? Comment, bool OutsideLimits = false) : ICommand<Result<CreatedEntry>>;

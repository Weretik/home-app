using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Planned.Commands.CreatePlannedExpense;

/// <summary>Plans an expense for a month. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreatePlannedExpenseCommand(
    string IdempotencyKey, int Year, int Month, string Name, Guid CategoryId, Payer Payer, decimal Amount)
    : ICommand<Result<CreatedEntry>>;

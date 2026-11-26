using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Budget;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;

/// <summary>Adds a spending limit at the end of the list. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateSpendingLimitCommand(
    string IdempotencyKey, string Name, LimitPeriod Period, decimal Amount, IReadOnlyList<Guid> CategoryIds)
    : ICommand<Result<CreatedEntry>>;

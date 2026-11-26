using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using Mediator;

namespace Expenses.Application.Features.Planned.Commands.RecordPlannedExpense;

/// <summary>
/// Turns a planned expense into a real one on <see cref="Date"/> and drops the plan, in one save, if nobody changed
/// the plan since <see cref="Version"/>. A repeat finds the plan gone (404) and records nothing twice.
/// </summary>
public sealed record RecordPlannedExpenseCommand(
    Guid Id, uint Version, DateOnly Date, PaymentMethod PaymentMethod, string? Comment)
    : ICommand<Result<CreatedEntry>>;

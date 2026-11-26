using BuildingBlocks.Domain.Results;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Planned.Commands.UpdatePlannedExpense;

/// <summary>Replaces a planned expense, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdatePlannedExpenseCommand(
    Guid Id, uint Version, int Year, int Month, string Name, Guid CategoryId, Payer Payer, decimal Amount)
    : ICommand<Result>;

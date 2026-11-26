using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;

/// <summary>Replaces a spending limit's name, period, amount and categories, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateSpendingLimitCommand(
    Guid Id, uint Version, string Name, LimitPeriod Period, decimal Amount, IReadOnlyList<Guid> CategoryIds) : ICommand<Result>;

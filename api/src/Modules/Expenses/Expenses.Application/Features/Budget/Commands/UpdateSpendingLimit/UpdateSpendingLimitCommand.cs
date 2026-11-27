using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;

/// <summary>
/// Replaces a spending limit's name, period and categories and sets its amount from the period that holds
/// <see cref="AmountFrom"/> on (null: the current period in Kyiv), if nobody changed it since <see cref="Version"/>.
/// </summary>
public sealed record UpdateSpendingLimitCommand(
    Guid Id, uint Version, string Name, LimitPeriod Period, decimal Amount, IReadOnlyList<Guid> CategoryIds, DateOnly? AmountFrom = null)
    : ICommand<Result>;

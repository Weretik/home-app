using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;

/// <summary>
/// Sets the savings goal, the day limits start counting (null keeps it) and the monthly limit on all expenses from the
/// month that holds <see cref="MonthlyLimitFrom"/> on (null: the current month in Kyiv), if nobody changed them since
/// <see cref="Version"/>.
/// </summary>
public sealed record UpdateBudgetSettingsCommand(
    uint Version, decimal MonthlyLimit, decimal SavingsGoal, DateOnly? LimitsFrom, DateOnly? MonthlyLimitFrom = null) : ICommand<Result>;

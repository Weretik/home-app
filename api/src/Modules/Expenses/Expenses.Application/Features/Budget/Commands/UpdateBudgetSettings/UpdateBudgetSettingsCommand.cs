using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;

/// <summary>
/// Sets the monthly limit on all expenses, the savings goal and the day limits start counting (null keeps it), if
/// nobody changed them since <see cref="Version"/>.
/// </summary>
public sealed record UpdateBudgetSettingsCommand(uint Version, decimal MonthlyLimit, decimal SavingsGoal, DateOnly? LimitsFrom) : ICommand<Result>;

using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;

/// <summary>Sets the monthly limit on all expenses and the savings goal, if nobody changed them since <see cref="Version"/>.</summary>
public sealed record UpdateBudgetSettingsCommand(uint Version, decimal MonthlyLimit, decimal SavingsGoal) : ICommand<Result>;

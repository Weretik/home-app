using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Incomes.Commands.DeleteIncome;

/// <summary>Deletes the entry for good.</summary>
public sealed record DeleteIncomeCommand(Guid Id) : ICommand<Result>;

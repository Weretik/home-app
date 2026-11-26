using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Planned.Commands.DeletePlannedExpense;

/// <summary>Drops a planned expense for good; nothing is recorded.</summary>
public sealed record DeletePlannedExpenseCommand(Guid Id) : ICommand<Result>;

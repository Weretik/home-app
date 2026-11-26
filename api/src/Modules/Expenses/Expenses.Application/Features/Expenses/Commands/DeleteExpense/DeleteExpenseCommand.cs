using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Expenses.Commands.DeleteExpense;

/// <summary>Deletes the entry for good.</summary>
public sealed record DeleteExpenseCommand(Guid Id) : ICommand<Result>;

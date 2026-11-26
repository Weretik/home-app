using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Savings.Commands.DeleteSavingsMovement;

/// <summary>Deletes the entry for good.</summary>
public sealed record DeleteSavingsMovementCommand(Guid Id) : ICommand<Result>;

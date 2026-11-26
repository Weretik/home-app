using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Savings;
using Mediator;

namespace Expenses.Application.Features.Savings.Commands.UpdateSavingsMovement;

/// <summary>Replaces the editable fields of a savings movement, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateSavingsMovementCommand(Guid Id, uint Version, DateOnly Date, Person Person, decimal Amount, string? Comment) : ICommand<Result>;

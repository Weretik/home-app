using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Expenses.Domain.Savings;
using Mediator;

namespace Expenses.Application.Features.Savings.Commands.CreateSavingsMovement;

/// <summary>Puts money into the savings jar (positive amount) or takes it out (negative). A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateSavingsMovementCommand(string IdempotencyKey, DateOnly Date, Person Person, decimal Amount, string? Comment) : ICommand<Result<CreatedEntry>>;

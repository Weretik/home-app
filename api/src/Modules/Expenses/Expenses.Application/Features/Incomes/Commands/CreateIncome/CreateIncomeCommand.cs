using BuildingBlocks.Domain.Results;
using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Incomes.Commands.CreateIncome;

/// <summary>Adds an income. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateIncomeCommand(string IdempotencyKey, DateOnly Date, Person Person, decimal Amount, string? Comment) : ICommand<Result<CreatedEntry>>;

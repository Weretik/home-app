using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Incomes.Commands.UpdateIncome;

/// <summary>Replaces the editable fields of an income, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateIncomeCommand(Guid Id, uint Version, DateOnly Date, Person Person, decimal Amount, string? Comment) : ICommand<Result>;

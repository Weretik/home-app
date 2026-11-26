using BuildingBlocks.Domain.Results;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Expenses.Commands.UpdateExpense;

/// <summary>Replaces the editable fields of an expense, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateExpenseCommand(Guid Id, uint Version, DateOnly Date, Payer Payer, Guid CategoryId, decimal Amount, PaymentMethod PaymentMethod, string? Comment) : ICommand<Result>;

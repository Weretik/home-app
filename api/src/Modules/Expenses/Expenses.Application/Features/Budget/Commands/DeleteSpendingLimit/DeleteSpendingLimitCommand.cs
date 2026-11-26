using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Budget.Commands.DeleteSpendingLimit;

/// <summary>Deletes a spending limit for good; its expenses stay.</summary>
public sealed record DeleteSpendingLimitCommand(Guid Id) : ICommand<Result>;

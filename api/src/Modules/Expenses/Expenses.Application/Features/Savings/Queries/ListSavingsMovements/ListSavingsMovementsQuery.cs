using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMovements;

/// <summary>The savings movements of one calendar month, newest first, with the balances around it.</summary>
public sealed record ListSavingsMovementsQuery(int Year, int Month, int Page, int PageSize)
    : IQuery<Result<SavingsMovementList>>;

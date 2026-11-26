using BuildingBlocks.Domain.Results;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Incomes.Queries.ListIncomes;

/// <summary>The incomes of one calendar month, newest first, optionally of one person.</summary>
public sealed record ListIncomesQuery(int Year, int Month, Person? Person, int Page, int PageSize)
    : IQuery<Result<IncomeList>>;

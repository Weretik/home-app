using BuildingBlocks.Domain.Results;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

/// <summary>The expenses of one calendar month, newest first, optionally of one category or payer.</summary>
public sealed record ListExpensesQuery(
    int Year,
    int Month,
    Guid? CategoryId,
    Payer? Payer,
    int Page,
    int PageSize) : IQuery<Result<ExpenseList>>;

using BuildingBlocks.Domain.Results;
using Expenses.Domain.Parties;
using Mediator;

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

/// <summary>
/// The expenses of one calendar month (<see cref="Year"/> and <see cref="Month"/>) or of a period of days
/// (<see cref="From"/> to <see cref="To"/>, both included), newest first, optionally of one category or payer.
/// </summary>
public sealed record ListExpensesQuery(
    int? Year,
    int? Month,
    DateOnly? From,
    DateOnly? To,
    Guid? CategoryId,
    Payer? Payer,
    int Page,
    int PageSize) : IQuery<Result<ExpenseList>>
{
    /// <summary>The first day and the day after the last one; call only on a valid query.</summary>
    public (DateOnly Start, DateOnly End) Period() =>
        From is { } from && To is { } to
            ? (from, to.AddDays(1))
            : (new DateOnly(Year!.Value, Month!.Value, 1), new DateOnly(Year!.Value, Month!.Value, 1).AddMonths(1));
}

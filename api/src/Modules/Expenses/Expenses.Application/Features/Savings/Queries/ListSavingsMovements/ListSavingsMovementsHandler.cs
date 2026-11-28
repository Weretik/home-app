using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Savings;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMovements;

public sealed class ListSavingsMovementsHandler(IExpensesDbContext db)
    : IQueryHandler<ListSavingsMovementsQuery, Result<SavingsMovementList>>
{
    public async ValueTask<Result<SavingsMovementList>> Handle(
        ListSavingsMovementsQuery query, CancellationToken cancellationToken)
    {
        var from = new DateOnly(query.Year, query.Month, 1);
        var to = from.AddMonths(1);
        var all = db.SavingsMovements.AsNoTracking();
        var month = all.Where(m => m.Date >= from && m.Date < to);

        var before = await all.Where(m => m.Date < from).SumAsync(m => m.Amount, cancellationToken);
        var deposits = await month.Where(m => m.Amount > 0).SumAsync(m => m.Amount, cancellationToken);
        var withdrawals = -await month.Where(m => m.Amount < 0).SumAsync(m => m.Amount, cancellationToken);
        var current = await all.SumAsync(m => m.Amount, cancellationToken);

        var page = await month
            .OrderByDescending(m => m.Date)
            .ThenByDescending(m => m.Id)
            .Select(m => new SavingsMovementItem(
                m.Id.Value,
                m.Version,
                m.Date,
                m.Person,
                m.Amount,
                m.Amount > 0 ? SavingsDirection.Deposit : SavingsDirection.Withdrawal,
                m.Comment))
            .ToPagedListAsync(query.Page, query.PageSize, cancellationToken);

        return new SavingsMovementList(
            page.Items,
            page.Page,
            page.PageSize,
            page.TotalCount,
            deposits,
            withdrawals,
            before,
            before + deposits - withdrawals,
            current);
    }
}

using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Savings.Queries.ListSavingsMonths;

/// <summary>Sums per month in SQL (one row per month), then the running balance in memory.</summary>
public sealed class ListSavingsMonthsHandler(IExpensesDbContext db)
    : IQueryHandler<ListSavingsMonthsQuery, Result<SavingsOverview>>
{
    public async ValueTask<Result<SavingsOverview>> Handle(ListSavingsMonthsQuery query, CancellationToken cancellationToken)
    {
        var months = await db.SavingsMovements
            .AsNoTracking()
            .GroupBy(m => new { m.Date.Year, m.Date.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Deposits = g.Sum(m => m.Amount > 0 ? m.Amount : 0),
                Withdrawals = -g.Sum(m => m.Amount < 0 ? m.Amount : 0),
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToListAsync(cancellationToken);

        var balance = 0m;
        var summaries = new List<SavingsMonthSummary>(months.Count);
        foreach (var month in months)
        {
            balance += month.Deposits - month.Withdrawals;
            summaries.Add(new SavingsMonthSummary(month.Year, month.Month, month.Deposits, month.Withdrawals, balance));
        }

        summaries.Reverse();
        return new SavingsOverview(balance, summaries);
    }
}

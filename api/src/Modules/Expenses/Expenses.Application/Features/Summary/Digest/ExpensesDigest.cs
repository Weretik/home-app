using Expenses.Application.Features.Summary.Queries.GetMonthSummary;
using Expenses.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Summary.Digest;

/// <summary>
/// The Expenses side of <see cref="IExpensesDigest"/>, for the Telegram summaries. The month report is the month
/// summary query itself (no second copy of the limit and carry-over arithmetic); the period report is three grouped
/// sums over the days asked for.
/// </summary>
public sealed class ExpensesDigest(IExpensesDbContext db, ISender sender) : IExpensesDigest
{
    private const int TopCategoryCount = 5;

    public async Task<ExpensesPeriodReport> GetPeriodAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var expenses = db.Expenses.AsNoTracking().Where(e => e.Date >= from && e.Date <= to);
        var totals = await expenses
            .GroupBy(e => 1)
            .Select(g => new { Count = g.Count(), Amount = g.Sum(e => e.Amount) })
            .FirstOrDefaultAsync(cancellationToken);
        var byPayer = await expenses
            .GroupBy(e => e.Payer)
            .Select(g => new { Payer = g.Key, Amount = g.Sum(e => e.Amount) })
            .ToListAsync(cancellationToken);
        var byCategory = await expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(e => e.Amount) })
            .Join(db.Categories.AsNoTracking(), x => x.CategoryId, c => c.Id, (x, c) => new { c.Name, x.Amount })
            .OrderByDescending(x => x.Amount)
            .Take(TopCategoryCount)
            .ToListAsync(cancellationToken);

        return new ExpensesPeriodReport(
            from,
            to,
            totals?.Amount ?? 0,
            totals?.Count ?? 0,
            [.. byPayer.OrderByDescending(x => x.Amount).Select(x => new NamedAmount(x.Payer.ToString(), x.Amount))],
            [.. byCategory.Select(x => new NamedAmount(x.Name, x.Amount))]);
    }

    public async Task<ExpensesMonthReport> GetMonthAsync(int year, int month, CancellationToken cancellationToken)
    {
        var summary = await sender.Send(new GetMonthSummaryQuery(year, month), cancellationToken);
        return summary.IsSuccess ? DigestReports.ToReport(summary.Value) : DigestReports.Empty(year, month);
    }
}

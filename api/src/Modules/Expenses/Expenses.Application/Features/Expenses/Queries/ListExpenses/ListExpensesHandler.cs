using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

public sealed class ListExpensesHandler(IExpensesDbContext db) : IQueryHandler<ListExpensesQuery, Result<ExpenseList>>
{
    public async ValueTask<Result<ExpenseList>> Handle(ListExpensesQuery query, CancellationToken cancellationToken)
    {
        var (from, to) = query.Period();

        var expenses = db.Expenses.AsNoTracking().Where(e => e.Date >= from && e.Date < to);
        if (query.CategoryId is { } categoryId)
            expenses = expenses.Where(e => e.CategoryId == categoryId);
        if (query.Payer is { } payer)
            expenses = expenses.Where(e => e.Payer == payer);

        var total = await expenses.SumAsync(e => e.Amount, cancellationToken);
        var page = await expenses
            .Join(db.Categories, e => e.CategoryId, c => c.Id, (e, c) => new { Expense = e, CategoryName = c.Name })
            .OrderByDescending(x => x.Expense.Date)
            .ThenByDescending(x => x.Expense.Id)
            .Select(x => new ExpenseItem(
                x.Expense.Id,
                x.Expense.Version,
                x.Expense.Date,
                x.Expense.Payer,
                x.Expense.CategoryId,
                x.CategoryName,
                x.Expense.Amount,
                x.Expense.PaymentMethod,
                x.Expense.Comment))
            .ToPagedListAsync(query.Page, query.PageSize, cancellationToken);

        return new ExpenseList(page.Items, page.Page, page.PageSize, page.TotalCount, total);
    }
}

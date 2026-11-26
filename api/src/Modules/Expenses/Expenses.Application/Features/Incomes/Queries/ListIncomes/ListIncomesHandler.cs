using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application.Features.Incomes.Queries.ListIncomes;

public sealed class ListIncomesHandler(IExpensesDbContext db) : IQueryHandler<ListIncomesQuery, Result<IncomeList>>
{
    public async ValueTask<Result<IncomeList>> Handle(ListIncomesQuery query, CancellationToken cancellationToken)
    {
        var from = new DateOnly(query.Year, query.Month, 1);
        var to = from.AddMonths(1);

        var incomes = db.Incomes.AsNoTracking().Where(i => i.Date >= from && i.Date < to);
        if (query.Person is { } person)
            incomes = incomes.Where(i => i.Person == person);

        var total = await incomes.SumAsync(i => i.Amount, cancellationToken);
        var page = await incomes
            .OrderByDescending(i => i.Date)
            .ThenByDescending(i => i.Id)
            .Select(i => new IncomeItem(i.Id, i.Date, i.Person, i.Amount, i.Comment))
            .ToPagedListAsync(query.Page, query.PageSize, cancellationToken);

        return new IncomeList(page.Items, page.Page, page.PageSize, page.TotalCount, total);
    }
}

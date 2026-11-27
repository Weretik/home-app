using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Items.Queries.ListFrequent;

public sealed class ListFrequentHandler(IShoppingDbContext db, TimeProvider clock)
    : IQueryHandler<ListFrequentQuery, Result<IReadOnlyList<FrequentItem>>>
{
    public const int Count = 20;

    public async ValueTask<Result<IReadOnlyList<FrequentItem>>> Handle(ListFrequentQuery query, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var top = await FrequentStatistics.TopAsync(db, window, Count, cancellationToken);
        var departments = await db.Departments.AsNoTracking().ToDictionaryAsync(d => d.Id, cancellationToken);

        return top
            .Select(f => new FrequentItem(
                f.Name, f.DepartmentId, departments[new DepartmentId(f.DepartmentId)].Name, departments[new DepartmentId(f.DepartmentId)].Icon, f.TimesBought, f.LastBoughtAtUtc))
            .ToList();
    }
}

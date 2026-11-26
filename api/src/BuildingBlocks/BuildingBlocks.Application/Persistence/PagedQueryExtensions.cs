using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Persistence;

public static class PagedQueryExtensions
{
    /// <summary>Counts and cuts the page in SQL; the query must already be ordered.</summary>
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedList<T>(items, page, pageSize, total);
    }
}

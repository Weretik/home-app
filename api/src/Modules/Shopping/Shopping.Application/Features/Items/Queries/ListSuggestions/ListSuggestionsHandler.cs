using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;
using Shopping.Domain.Parsing;

namespace Shopping.Application.Features.Items.Queries.ListSuggestions;

/// <summary>
/// Three reads: the top names (an escaped <c>LIKE</c> on the normalised name, grouped and ranked in SQL), then the
/// latest row with its department and the "in the list" flag of only those names. The first read scans the table: a prefix or word-start
/// <c>LIKE</c> cannot use the btree index, and the table holds a few thousand rows (no trigram index: production
/// refuses <c>pg_trgm</c>).
/// </summary>
public sealed class ListSuggestionsHandler(IShoppingDbContext db, TimeProvider clock)
    : IQueryHandler<ListSuggestionsQuery, Result<IReadOnlyList<NameSuggestion>>>
{
    public async ValueTask<Result<IReadOnlyList<NameSuggestion>>> Handle(
        ListSuggestionsQuery query, CancellationToken cancellationToken)
    {
        var text = NameNormalizer.Normalize(query.Query);
        var starts = SearchPattern.StartsWith(text);
        var wordStarts = SearchPattern.WordStartsWith(text);
        var escape = SearchPattern.EscapeCharacter;
        var items = db.Items.AsNoTracking();

        var top = await items
            .Where(i => EF.Functions.Like(i.NormalizedName, starts, escape)
                || EF.Functions.Like(i.NormalizedName, wordStarts, escape))
            .GroupBy(i => i.NormalizedName)
            .Select(g => new
            {
                Key = g.Key,
                Starts = EF.Functions.Like(g.Key, starts, escape),
                Times = g.Count(i => i.Status == ShoppingItemStatus.Bought),
                Last = g.Max(i => i.AddedAtUtc),
            })
            .OrderByDescending(g => g.Starts)
            .ThenByDescending(g => g.Times)
            .ThenByDescending(g => g.Last)
            .ThenBy(g => g.Key)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
        if (top.Count == 0)
            return Array.Empty<NameSuggestion>();

        var keys = top.Select(t => t.Key).ToList();
        // The latest row of each name with its department, in one statement (one snapshot: no department or row can
        // disappear between two reads).
        var latest = await items
            .Where(i => keys.Contains(i.NormalizedName))
            .Join(db.Departments.AsNoTracking(), i => i.DepartmentId, d => d.Id, (i, d) => new
            {
                i.NormalizedName,
                i.Name,
                i.DepartmentId,
                i.AddedAtUtc,
                i.Id,
                DepartmentName = d.Name,
                DepartmentIcon = d.Icon,
            })
            .GroupBy(x => x.NormalizedName)
            .Select(g => g.OrderByDescending(x => x.AddedAtUtc).ThenByDescending(x => x.Id).First())
            .ToDictionaryAsync(x => x.NormalizedName, cancellationToken);
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var inList = (await items
                .Where(i => keys.Contains(i.NormalizedName))
                .Where(CartWindow.InList(window.Cutoff))
                .Select(i => i.NormalizedName)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();

        // A name whose rows were deleted between the two reads is left out, not an error.
        return top
            .Where(t => latest.ContainsKey(t.Key))
            .Select(t =>
            {
                var row = latest[t.Key];
                return new NameSuggestion(
                    row.Name, row.DepartmentId, row.DepartmentName, row.DepartmentIcon, t.Times, t.Last, inList.Contains(t.Key));
            })
            .ToList();
    }
}

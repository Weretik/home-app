using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Parsing;

namespace Shopping.Application.Features.Keywords.Queries.ListKeywords;

/// <summary>
/// The whole dictionary is a few hundred rows, so the list is not paged. The search is one escaped <c>LIKE</c> per
/// word on the (already lower-case, trigram-indexed) keyword column.
/// </summary>
public sealed class ListKeywordsHandler(IShoppingDbContext db)
    : IQueryHandler<ListKeywordsQuery, Result<IReadOnlyList<KeywordItem>>>
{
    public async ValueTask<Result<IReadOnlyList<KeywordItem>>> Handle(ListKeywordsQuery query, CancellationToken cancellationToken)
    {
        var keywords = db.Keywords.AsNoTracking();
        if (query.DepartmentId is { } departmentId)
            keywords = keywords.Where(k => k.DepartmentId == departmentId);
        foreach (var pattern in SearchPattern.ContainsEachWord(NameNormalizer.Normalize(query.Search)))
            keywords = keywords.Where(k => EF.Functions.Like(k.Keyword, pattern, SearchPattern.EscapeCharacter));

        return await keywords
            .Join(db.Departments.AsNoTracking(), k => k.DepartmentId, d => d.Id, (k, d) => new { k, d })
            .OrderBy(x => x.k.Keyword)
            .Select(x => new KeywordItem(x.k.Id, x.k.Keyword, x.k.DepartmentId, x.d.Name, x.d.Icon, x.k.Learned, x.k.Version))
            .ToListAsync(cancellationToken);
    }
}

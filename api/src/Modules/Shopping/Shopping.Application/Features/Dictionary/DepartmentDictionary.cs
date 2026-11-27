using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Matching;

namespace Shopping.Application.Features.Dictionary;

/// <summary>
/// The departments and keywords as one matcher (the bot's «Справочник»). Keywords are handed over in store order of
/// their department, then alphabetically, so a tie goes to the department met first in the store.
/// </summary>
public sealed class DepartmentDictionary
{
    private DepartmentDictionary(IReadOnlyDictionary<DepartmentId, Department> departments, DepartmentMatcher matcher)
    {
        Departments = departments;
        Matcher = matcher;
    }

    public IReadOnlyDictionary<DepartmentId, Department> Departments { get; }

    public DepartmentMatcher Matcher { get; }

    public static async Task<DepartmentDictionary> LoadAsync(IShoppingDbContext db, CancellationToken cancellationToken)
    {
        var departments = await db.Departments.AsNoTracking().ToDictionaryAsync(d => d.Id, cancellationToken);
        var keywords = await db.Keywords.AsNoTracking()
            .Select(k => new KeywordRule(k.Keyword, k.DepartmentId))
            .ToListAsync(cancellationToken);
        // Exactly one fallback is a schema invariant seeded by the first migration (ADR 0031), not a business rule,
        // so a missing one fails like the other seeded reads (DeleteDepartment's SingleAsync).
        var fallback = departments.Values.Single(d => d.IsFallback);

        var rules = keywords
            .OrderBy(k => departments[k.DepartmentId].SortOrder)
            .ThenBy(k => departments[k.DepartmentId].Name, StringComparer.Ordinal)
            .ThenBy(k => k.Keyword, StringComparer.Ordinal);
        return new DepartmentDictionary(departments, new DepartmentMatcher(rules, fallback.Id));
    }
}

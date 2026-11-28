using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Dictionary;

/// <summary>
/// The bot's learning: when a person puts an item into another department, its normalised name becomes a learned
/// keyword of that department (an existing keyword with that text is moved there). Changes are tracked; the
/// caller saves them with its own changes.
/// </summary>
public static class KeywordLearning
{
    public static async Task LearnAsync(
        IShoppingDbContext db, string normalizedName, Guid departmentId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!DepartmentKeyword.IsValid(normalizedName))
            return;

        var existing = await db.Keywords.FirstOrDefaultAsync(k => k.Keyword == normalizedName, cancellationToken);
        if (existing is not null)
        {
            existing.Learn(new DepartmentId(departmentId));
            return;
        }

        db.Keywords.Add(DepartmentKeyword.Create(normalizedName, new DepartmentId(departmentId), learned: true, now).Value);
    }
}

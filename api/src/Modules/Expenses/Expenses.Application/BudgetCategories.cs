using Microsoft.EntityFrameworkCore;

namespace Expenses.Application;

/// <summary>Checks that the categories a spending limit counts exist.</summary>
internal static class BudgetCategories
{
    /// <summary>The first id that is not a stored category, or null when all exist.</summary>
    public static async Task<Guid?> FirstMissingAsync(
        IExpensesDbContext db, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var stored = await db.Categories.Where(c => ids.Contains(c.Id)).Select(c => c.Id).ToListAsync(cancellationToken);
        return ids.Where(id => !stored.Contains(id)).Select(id => (Guid?)id).FirstOrDefault();
    }
}

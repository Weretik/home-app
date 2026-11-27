using Expenses.Domain.Categories;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application;

/// <summary>Checks that the categories a spending limit counts exist.</summary>
internal static class BudgetCategories
{
    /// <summary>The first id that is not a stored category, or null when all exist.</summary>
    public static async Task<CategoryId?> FirstMissingAsync(
        IExpensesDbContext db, IReadOnlyCollection<CategoryId> ids, CancellationToken cancellationToken)
    {
        var stored = await db.Categories.Where(c => ids.Contains(c.Id)).Select(c => c.Id).ToListAsync(cancellationToken);
        return ids.Where(id => !stored.Contains(id)).Select(id => (CategoryId?)id).FirstOrDefault();
    }
}

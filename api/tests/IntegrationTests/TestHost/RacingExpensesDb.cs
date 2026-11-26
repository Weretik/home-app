using Expenses.Application;
using Expenses.Application.Idempotency;
using Expenses.Domain.Budget;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Planning;
using Expenses.Domain.Savings;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.TestHost;

/// <summary>
/// The module's unit of work with a race built in: <paramref name="meanwhile"/> runs once, right before the save,
/// after the handler has read and checked its rows. That is the window a second request can hit, so the race guards
/// (row version, unique index) can be tested deterministically on the real database.
/// </summary>
internal sealed class RacingExpensesDb(IExpensesDbContext inner, Func<Task> meanwhile) : IExpensesDbContext
{
    private bool _raced;

    public DbSet<Category> Categories => inner.Categories;

    public DbSet<Expense> Expenses => inner.Expenses;

    public DbSet<Income> Incomes => inner.Incomes;

    public DbSet<SavingsMovement> SavingsMovements => inner.SavingsMovements;

    public DbSet<CreateRequest> CreateRequests => inner.CreateRequests;

    public DbSet<SpendingLimit> SpendingLimits => inner.SpendingLimits;

    public DbSet<BudgetSettings> BudgetSettings => inner.BudgetSettings;

    public DbSet<PlannedExpense> PlannedExpenses => inner.PlannedExpenses;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_raced)
        {
            _raced = true;
            await meanwhile();
        }

        return await inner.SaveChangesAsync(cancellationToken);
    }
}

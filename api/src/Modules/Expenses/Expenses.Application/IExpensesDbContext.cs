using Expenses.Application.Idempotency;
using Expenses.Domain.Budget;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Planning;
using Expenses.Domain.Savings;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Application;

/// <summary>
/// The module's unit of work: handlers query the sets and call <see cref="SaveChangesAsync"/> once. Infrastructure
/// implements it with the EF Core context that lives in the <c>expenses</c> schema.
/// </summary>
public interface IExpensesDbContext
{
    DbSet<Category> Categories { get; }

    DbSet<Expense> Expenses { get; }

    DbSet<Income> Incomes { get; }

    DbSet<SavingsMovement> SavingsMovements { get; }

    DbSet<CreateRequest> CreateRequests { get; }

    DbSet<SpendingLimit> SpendingLimits { get; }

    DbSet<BudgetSettings> BudgetSettings { get; }

    DbSet<PlannedExpense> PlannedExpenses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

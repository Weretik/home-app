using BuildingBlocks.Infrastructure.Persistence;
using Expenses.Application;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Microsoft.EntityFrameworkCore;

namespace Expenses.Infrastructure;

/// <summary>The module's tables, all in the <c>expenses</c> schema, with their own migration history.</summary>
public sealed class ExpensesDbContext(DbContextOptions<ExpensesDbContext> options)
    : ModuleDbContext(options, Schema), IExpensesDbContext
{
    public const string Schema = "expenses";

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Income> Incomes => Set<Income>();
}

using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shopping.Application;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Settings;

namespace Shopping.Infrastructure;

/// <summary>The module's tables, all in the <c>shopping</c> schema, with their own migration history.</summary>
public sealed class ShoppingDbContext(DbContextOptions<ShoppingDbContext> options)
    : ModuleDbContext(options, Schema), IShoppingDbContext
{
    public const string Schema = "shopping";

    public DbSet<ShoppingItem> Items => Set<ShoppingItem>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<DepartmentKeyword> Keywords => Set<DepartmentKeyword>();

    public DbSet<ShoppingSettings> Settings => Set<ShoppingSettings>();

    public DbSet<CreateRequest> CreateRequests => Set<CreateRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Trigram index on keywords for the settings search (docs/rules.md, Data).
        modelBuilder.HasPostgresExtension("pg_trgm");
        base.OnModelCreating(modelBuilder);
    }
}

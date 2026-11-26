using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Settings;

namespace Shopping.Application;

/// <summary>
/// The module's unit of work: handlers query the sets and call <see cref="SaveChangesAsync"/> once. Infrastructure
/// implements it with the EF Core context that lives in the <c>shopping</c> schema. <see cref="Database"/> is there
/// for the one command that needs an explicit transaction (deleting a department moves its rows first).
/// </summary>
public interface IShoppingDbContext
{
    DbSet<ShoppingItem> Items { get; }

    DbSet<Department> Departments { get; }

    DbSet<DepartmentKeyword> Keywords { get; }

    DbSet<ShoppingSettings> Settings { get; }

    DbSet<CreateRequest> CreateRequests { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Todos.Application.Idempotency;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application;

/// <summary>
/// The module's unit of work: handlers query the sets and call <see cref="SaveChangesAsync"/> once. Infrastructure
/// implements it with the EF Core context that lives in the <c>todos</c> schema.
/// </summary>
public interface ITodosDbContext
{
    DbSet<TodoTask> Tasks { get; }

    DbSet<TodoCategory> Categories { get; }

    DbSet<CreateRequest> CreateRequests { get; }

    /// <summary>Lets a handler that lost a race on a row version forget what it read and read again.</summary>
    ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

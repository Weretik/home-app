using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Todos.Application;
using Todos.Application.Idempotency;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Infrastructure;

/// <summary>The module's tables, all in the <c>todos</c> schema, with their own migration history.</summary>
public sealed class TodosDbContext(DbContextOptions<TodosDbContext> options)
    : ModuleDbContext(options, Schema), ITodosDbContext
{
    public const string Schema = "todos";

    /// <summary>The task-completed events for Gifts go out through the module's outbox (ADR 0035).</summary>
    protected override bool HasOutbox => true;

    public DbSet<TodoTask> Tasks => Set<TodoTask>();

    public DbSet<TodoCategory> Categories => Set<TodoCategory>();

    public DbSet<CreateRequest> CreateRequests => Set<CreateRequest>();
}

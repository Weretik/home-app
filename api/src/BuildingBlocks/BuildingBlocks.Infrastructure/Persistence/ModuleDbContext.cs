using BuildingBlocks.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The base of every module DbContext: the module keeps its tables in its own schema, and a unique index
/// violation comes out as <see cref="UniqueConstraintViolationException"/>.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, string schema) : DbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
        {
            throw new UniqueConstraintViolationException(postgres.ConstraintName, exception);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(schema);
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

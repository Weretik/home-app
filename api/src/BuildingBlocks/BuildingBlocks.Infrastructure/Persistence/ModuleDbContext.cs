using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Infrastructure.Outbox.Dispatch;
using BuildingBlocks.Infrastructure.Outbox.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// The base of every module DbContext: the module keeps its tables in its own schema, and a unique index
/// violation comes out as <see cref="UniqueConstraintViolationException"/>. A module that publishes messages turns on
/// <see cref="HasOutbox"/>, which maps the shared outbox table into its schema, and calls <see cref="Publish"/>.
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options, string schema) : DbContext(options)
{
    /// <summary>True for a module that writes outbox messages (ADR 0035).</summary>
    protected virtual bool HasOutbox => false;

    /// <summary>Adds an outbox row for <paramref name="message"/>; it is saved with the data by the next save.</summary>
    public void Publish(object message)
    {
        if (!HasOutbox)
            throw new InvalidOperationException($"{GetType().Name} has no outbox; override {nameof(HasOutbox)}.");

        // AddDbContext hands EF the application's service provider; the registry of message types lives there.
        var services = this.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider
            ?? throw new InvalidOperationException("The DbContext was not created by dependency injection.");
        Set<OutboxMessage>().Add(services.GetRequiredService<OutboxMessageTypes>().CreateRow(message));
    }

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
        if (HasOutbox)
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
    }
}

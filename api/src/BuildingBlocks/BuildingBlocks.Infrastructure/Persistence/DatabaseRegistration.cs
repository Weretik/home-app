using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BuildingBlocks.Infrastructure.Persistence;

public static class DatabaseRegistration
{
    public const string ConnectionStringName = "Database";

    /// <summary>
    /// Registers a module DbContext on the shared PostgreSQL database. The module keeps its migration history in
    /// its own schema, so modules migrate independently and one can later move to its own database.
    /// </summary>
    public static IServiceCollection AddModuleDbContext<TContext>(
        this IServiceCollection services, IConfiguration configuration, string schema)
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not set.");

        services.AddDbContext<TContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", schema)));
        return services;
    }

    /// <summary>Adds the database check when <c>ConnectionStrings:Database</c> is set; without it the API runs without a database.</summary>
    public static IHealthChecksBuilder AddDatabaseCheck(this IHealthChecksBuilder checks, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DatabaseRegistration.ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(connectionString))
            checks.Add(new HealthCheckRegistration(DatabaseHealthCheck.Name, _ => new DatabaseHealthCheck(connectionString), null, null));
        return checks;
    }
}

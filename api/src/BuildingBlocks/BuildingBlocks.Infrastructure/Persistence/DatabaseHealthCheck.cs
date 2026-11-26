using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>Opens a connection to the shared database and runs <c>SELECT 1</c>.</summary>
public sealed class DatabaseHealthCheck(string connectionString) : IHealthCheck
{
    public const string Name = "database";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException or InvalidOperationException)
        {
            return HealthCheckResult.Unhealthy("The database is unreachable.", exception);
        }
    }
}

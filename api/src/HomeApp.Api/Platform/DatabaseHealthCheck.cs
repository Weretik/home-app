using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace HomeApp.Api.Platform;

/// <summary>Opens a connection and runs <c>SELECT 1</c>.</summary>
internal sealed class DatabaseHealthCheck(string connectionString) : IHealthCheck
{
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
        catch (Exception exception) when (exception is NpgsqlException or TimeoutException)
        {
            return HealthCheckResult.Unhealthy("Database is unreachable.", exception);
        }
    }
}

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Prepares a module's database for local development: applies its migrations and adds sample data. Production
/// never runs it; there migrations are a separate step before the deploy.
/// </summary>
public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}

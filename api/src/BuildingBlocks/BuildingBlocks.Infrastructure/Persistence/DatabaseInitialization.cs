using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Persistence;

public static class DatabaseInitialization
{
    /// <summary>Runs the initializer of every module, in registration order.</summary>
    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        foreach (var initializer in scope.ServiceProvider.GetServices<IDatabaseInitializer>())
            await initializer.InitializeAsync(cancellationToken);
    }
}

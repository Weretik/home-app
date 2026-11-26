using BuildingBlocks.Infrastructure.Persistence;

namespace Host.Api.Composition;

/// <summary>
/// With <c>Database:InitializeOnStartup</c> (local development only) the host migrates every module and adds
/// sample data when it starts. In production migrations run as a separate step before the deploy.
/// </summary>
internal sealed class DatabaseStartup(IServiceProvider services) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => services.InitializeDatabasesAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public static void AddWhenEnabled(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // The build starts the host to write the OpenAPI document; that run must not touch a database.
        var generatingDocument = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
        if (configuration.GetValue<bool>("Database:InitializeOnStartup") && !generatingDocument)
            serviceCollection.AddHostedService<DatabaseStartup>();
    }
}

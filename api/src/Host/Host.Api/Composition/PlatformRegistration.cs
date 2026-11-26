using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using BuildingBlocks.Api.Scheduling;
using BuildingBlocks.Infrastructure.Persistence;

namespace Host.Api.Composition;

/// <summary>What every module shares: errors as problem details, the pipeline, CORS, rate limits, API docs, health checks.</summary>
internal static class PlatformRegistration
{
    public static IServiceCollection AddPlatform(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        // Schemes and policies come with an identity module; the middleware is already in place.
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddApiProblems();
        services.AddMediatorPipeline();
        services.AddReverseProxySupport(configuration);
        services.AddCorsForApps(configuration);
        services.AddApiRateLimiting(configuration);
        services.AddApiDocumentation();
        services.AddHealthChecks().AddDatabaseCheck(configuration);
        DatabaseStartup.AddWhenEnabled(services, configuration);
        services.Configure<OutboxTickOptions>(configuration.GetSection(OutboxTickOptions.SectionName));
        return services;
    }
}

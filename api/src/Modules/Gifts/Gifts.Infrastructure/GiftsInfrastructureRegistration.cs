using BuildingBlocks.Infrastructure.Persistence;
using Gifts.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gifts.Infrastructure;

public static class GiftsInfrastructureRegistration
{
    public static IServiceCollection AddGiftsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GiftsDbContext>(configuration, GiftsDbContext.Schema);
        services.AddScoped<IGiftsDbContext>(provider => provider.GetRequiredService<GiftsDbContext>());
        services.AddScoped<IDatabaseInitializer, GiftsDatabaseInitializer>();
        return services;
    }
}

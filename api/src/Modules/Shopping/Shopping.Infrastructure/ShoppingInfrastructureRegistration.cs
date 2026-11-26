using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping.Application;

namespace Shopping.Infrastructure;

public static class ShoppingInfrastructureRegistration
{
    public static IServiceCollection AddShoppingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ShoppingDbContext>(configuration, ShoppingDbContext.Schema);
        services.AddScoped<IShoppingDbContext>(provider => provider.GetRequiredService<ShoppingDbContext>());
        services.AddScoped<IDatabaseInitializer, ShoppingDatabaseInitializer>();
        return services;
    }
}

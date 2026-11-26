using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todos.Application;

namespace Todos.Infrastructure;

public static class TodosInfrastructureRegistration
{
    public static IServiceCollection AddTodosInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<TodosDbContext>(configuration, TodosDbContext.Schema);
        services.AddScoped<ITodosDbContext>(provider => provider.GetRequiredService<TodosDbContext>());
        services.AddScoped<IDatabaseInitializer, TodosDatabaseInitializer>();
        return services;
    }
}

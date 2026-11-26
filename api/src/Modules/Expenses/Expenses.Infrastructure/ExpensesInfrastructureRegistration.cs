using BuildingBlocks.Infrastructure.Persistence;
using Expenses.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Expenses.Infrastructure;

public static class ExpensesInfrastructureRegistration
{
    public static IServiceCollection AddExpensesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<ExpensesDbContext>(configuration, ExpensesDbContext.Schema);
        services.AddScoped<IExpensesDbContext>(provider => provider.GetRequiredService<ExpensesDbContext>());
        services.AddScoped<IDatabaseInitializer, ExpensesDatabaseInitializer>();
        return services;
    }
}

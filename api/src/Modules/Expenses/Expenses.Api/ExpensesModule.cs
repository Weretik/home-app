using Expenses.Api.Endpoints.Expenses;
using Expenses.Api.Endpoints.Import;
using Expenses.Api.Endpoints.Incomes;
using Expenses.Api.Endpoints.Savings;
using Expenses.Application;
using Expenses.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Expenses.Api;

/// <summary>
/// The Expenses module: money spent and received by the household, by category and person (the old «Учёт расходов»
/// sheet). The host calls <see cref="AddExpensesModule"/> and <see cref="MapExpensesModule"/>; nothing else of the
/// module is visible to it.
/// </summary>
public static class ExpensesModule
{
    public static IServiceCollection AddExpensesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExpensesApplication();
        services.AddExpensesInfrastructure(configuration);
        services.Configure<SheetImportOptions>(configuration.GetSection(SheetImportOptions.SectionName));
        return services;
    }

    public static IEndpointRouteBuilder MapExpensesModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/expenses")
            .WithTags("Expenses")
            .MapListExpenses()
            .MapCreateExpense()
            .MapUpdateExpense()
            .MapDeleteExpense()
            .MapListCategories()
            .MapListIncomes()
            .MapCreateIncome()
            .MapUpdateIncome()
            .MapDeleteIncome()
            .MapListSavingsMovements()
            .MapListSavingsMonths()
            .MapCreateSavingsMovement()
            .MapUpdateSavingsMovement()
            .MapDeleteSavingsMovement()
            .MapImportSheet();
        return endpoints;
    }
}

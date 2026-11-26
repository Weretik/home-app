using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shopping.Api.Endpoints.Departments;
using Shopping.Api.Endpoints.Items.Editing;
using Shopping.Api.Endpoints.Items.Reading;
using Shopping.Api.Endpoints.Keywords;
using Shopping.Api.Endpoints.Settings;
using Shopping.Application;
using Shopping.Infrastructure;

namespace Shopping.Api;

/// <summary>
/// The Shopping module: the shared shopping list with departments, a cart, history and frequent items (the old
/// «🛒 Список покупок» sheet and its Telegram bot). The host calls <see cref="AddShoppingModule"/> and
/// <see cref="MapShoppingModule"/>; nothing else of the module is visible to it.
/// </summary>
public static class ShoppingModule
{
    public static IServiceCollection AddShoppingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddShoppingApplication();
        services.AddShoppingInfrastructure(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapShoppingModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/shopping")
            .WithTags("Shopping")
            .MapListShoppingItems()
            .MapAddShoppingItems()
            .MapUpdateShoppingItem()
            .MapSetShoppingItemStatus()
            .MapDeleteShoppingItem()
            .MapClearShoppingCart()
            .MapListShoppingHistory()
            .MapListFrequentItems()
            .MapListShoppingNameSuggestions()
            .MapGetShoppingSummary()
            .MapListShoppingDepartments()
            .MapMatchShoppingDepartment()
            .MapCreateShoppingDepartment()
            .MapUpdateShoppingDepartment()
            .MapDeleteShoppingDepartment()
            .MapReorderShoppingDepartments()
            .MapListShoppingKeywords()
            .MapCreateShoppingKeyword()
            .MapUpdateShoppingKeyword()
            .MapDeleteShoppingKeyword()
            .MapGetShoppingSettings()
            .MapUpdateShoppingSettings();
        return endpoints;
    }
}

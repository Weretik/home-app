using BuildingBlocks.Infrastructure.Outbox;
using BuildingBlocks.Infrastructure.Persistence;
using Gifts.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todos.Application;
using Todos.Application.Integrations.Bot;
using Todos.Application.Integrations.Gifts;
using Todos.Contracts;

namespace Todos.Infrastructure;

public static class TodosInfrastructureRegistration
{
    public static IServiceCollection AddTodosInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<TodosDbContext>(configuration, TodosDbContext.Schema);
        services.AddScoped<ITodosDbContext>(provider => provider.GetRequiredService<TodosDbContext>());
        services.AddScoped<IDatabaseInitializer, TodosDatabaseInitializer>();

        // What the Telegram bot reads and does (Notifications module, through Todos.Contracts).
        services.AddScoped<ITodosDigest, TodosDigest>();
        services.AddScoped<ITodoCompletion, TodoCompletion>();

        // Todos publishes the task-completed event (handled by Gifts) and handles the purchase events of Gifts.
        services.AddModuleOutbox<TodosDbContext>();
        services.AddOutboxMessage<GiftPurchasePlanned, GiftPurchasePlannedHandler>("gifts.purchase_planned");
        services.AddOutboxMessage<GiftPurchaseClosed, GiftPurchaseClosedHandler>("gifts.purchase_closed");
        return services;
    }
}

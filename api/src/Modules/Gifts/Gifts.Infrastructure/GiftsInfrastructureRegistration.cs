using BuildingBlocks.Infrastructure.Outbox;
using BuildingBlocks.Infrastructure.Persistence;
using Gifts.Application;
using Gifts.Application.Purchases;
using Gifts.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todos.Contracts;

namespace Gifts.Infrastructure;

public static class GiftsInfrastructureRegistration
{
    /// <summary>Used when <c>Site:Url</c> is not set: the production web app.</summary>
    public const string DefaultSiteUrl = "https://ambitious-beach-0b1d13b10.6.azurestaticapps.net";

    public static IServiceCollection AddGiftsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<GiftsDbContext>(configuration, GiftsDbContext.Schema);
        services.AddScoped<IGiftsDbContext>(provider => provider.GetRequiredService<GiftsDbContext>());
        services.AddScoped<IDatabaseInitializer, GiftsDatabaseInitializer>();

        var site = configuration[GiftLinks.SettingName];
        services.AddSingleton(new GiftLinks(string.IsNullOrWhiteSpace(site) ? DefaultSiteUrl : site));
        services.AddScoped<IGiftsDigest, GiftsDigest>();

        // Gifts publishes purchase events (handled by Todos) and handles the task-completed event of Todos.
        services.AddModuleOutbox<GiftsDbContext>();
        services.AddOutboxMessage<TodoTaskCompleted, TodoTaskCompletedHandler>("todos.task_completed");
        return services;
    }
}

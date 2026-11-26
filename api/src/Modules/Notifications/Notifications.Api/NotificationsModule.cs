using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Api.Endpoints.Site;
using Notifications.Api.Endpoints.Telegram;
using Notifications.Application;
using Notifications.Infrastructure;

namespace Notifications.Api;

/// <summary>
/// The Notifications module: the family Telegram bot with its digests, reminders and linked chats (ADR 0033). The
/// host calls <see cref="AddNotificationsModule"/> and <see cref="MapNotificationsModule"/>; nothing else of the module
/// is visible to it.
/// </summary>
public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotificationsApplication();
        services.AddNotificationsInfrastructure(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapNotificationsModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .MapGetNotificationsStatus()
            .MapUnlinkNotificationsChat()
            .MapSendNotificationsTest();

        // Called by Telegram and by the scheduler with a secret header, hidden from the OpenAPI document.
        endpoints.MapTelegramWebhook();
        endpoints.MapNotificationsTick();
        return endpoints;
    }
}

using BuildingBlocks.Infrastructure.Outbox;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application;
using Notifications.Application.Sending;
using Notifications.Application.Push;
using Notifications.Infrastructure.Push;
using Notifications.Infrastructure.Telegram;

namespace Notifications.Infrastructure;

public static class NotificationsInfrastructureRegistration
{
    /// <summary>Used when <c>Site:Url</c> is not set: the production web app.</summary>
    public const string DefaultSiteUrl = "https://ambitious-beach-0b1d13b10.6.azurestaticapps.net";

    public static IServiceCollection AddNotificationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<NotificationsDbContext>(configuration, NotificationsDbContext.Schema);
        services.AddScoped<INotificationsDbContext>(provider => provider.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<IDatabaseInitializer, NotificationsDatabaseInitializer>();

        var site = configuration["Site:Url"];
        services.AddSingleton(new NotificationsSettings(string.IsNullOrWhiteSpace(site) ? DefaultSiteUrl : site));

        services.AddOptions<TelegramOptions>()
            .Bind(configuration.GetSection(TelegramOptions.SectionName))
            .PostConfigure(options => options.WebhookUrl = WebhookUrl(configuration));
        services.AddMemoryCache();
        services.AddHttpClient<ITelegramBot, TelegramBotClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.telegram.org/");
            client.Timeout = TimeSpan.FromSeconds(20);
        })
            // The request url carries the bot token; the factory's default loggers would write it to the logs.
            .RemoveAllLoggers();

        services.AddOptions<WebPushOptions>().Bind(configuration.GetSection(WebPushOptions.SectionName));
        services.AddHttpClient<IWebPush, WebPushClient>(client => client.Timeout = TimeSpan.FromSeconds(20))
            // A push endpoint's path addresses a device; the factory's default loggers would write it to the logs.
            .RemoveAllLoggers();

        services.AddModuleOutbox<NotificationsDbContext>();
        services.AddOutboxMessage<TelegramMessage, TelegramMessageHandler>(TelegramMessage.TypeName);
        services.AddOutboxMessage<PushMessage, PushMessageHandler>(PushMessage.TypeName);
        return services;
    }

    /// <summary><c>Notifications:ApiUrl</c>, else the App Service host name, plus the webhook path; empty when neither is set.</summary>
    private static string WebhookUrl(IConfiguration configuration)
    {
        var api = configuration["Notifications:ApiUrl"];
        if (string.IsNullOrWhiteSpace(api) && configuration["WEBSITE_HOSTNAME"] is { Length: > 0 } host)
            api = $"https://{host}";
        return string.IsNullOrWhiteSpace(api) ? string.Empty : api.TrimEnd('/') + TelegramOptions.WebhookPath;
    }
}

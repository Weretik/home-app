namespace Notifications.Infrastructure.Telegram;

/// <summary>
/// Settings of the bot: <c>Telegram:BotToken</c> (secret, App Service <c>Telegram__BotToken</c>) and the address
/// Telegram calls, <c>Notifications:ApiUrl</c> or else <c>https://{WEBSITE_HOSTNAME}</c> that App Service sets.
/// </summary>
public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    public const string WebhookPath = "/api/notifications/telegram/webhook";

    public string BotToken { get; set; } = string.Empty;

    /// <summary>The full webhook url; empty when the API's address is unknown (local runs).</summary>
    public string WebhookUrl { get; set; } = string.Empty;
}

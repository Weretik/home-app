using BuildingBlocks.Domain.Results;

namespace Notifications.Domain.Chats;

/// <summary>The rules the bot's chats and messages can break, with stable codes for the client.</summary>
public static class ChatErrors
{
    public static readonly Error NotConfigured = Error.Conflict(
        "notifications.not_configured", "Токен Telegram-бота не задано (налаштування App Service Telegram__BotToken).");

    public static readonly Error TelegramFailed =
        Error.Conflict("notifications.telegram_failed", "Telegram не прийняв повідомлення; спробуйте пізніше.");

    public static Error NotLinked(Audience audience) =>
        Error.NotFound("notifications.chat_not_linked", $"Для {Whom(audience)} не підключено чат Telegram.");

    private static string Whom(Audience audience) => audience switch
    {
        Audience.Vitaliy => "Віталія",
        Audience.Maria => "Маші",
        _ => "сімейної групи",
    };
}

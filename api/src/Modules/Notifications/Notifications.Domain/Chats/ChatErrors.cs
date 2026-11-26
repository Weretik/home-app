using BuildingBlocks.Domain.Results;

namespace Notifications.Domain.Chats;

/// <summary>The rules the bot's chats and messages can break, with stable codes for the client.</summary>
public static class ChatErrors
{
    public static readonly Error NotConfigured = Error.Conflict(
        "notifications.not_configured", "The Telegram bot token is not set (App Service setting Telegram__BotToken).");

    public static readonly Error TelegramFailed =
        Error.Conflict("notifications.telegram_failed", "Telegram did not accept the message; try again later.");

    public static Error NotLinked(Audience audience) =>
        Error.NotFound("notifications.chat_not_linked", $"No Telegram chat is linked for {audience}.");
}

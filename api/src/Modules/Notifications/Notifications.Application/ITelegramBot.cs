using Notifications.Application.Sending;

namespace Notifications.Application;

/// <summary>
/// The Telegram Bot API as the module needs it; Infrastructure implements it over HTTP. Texts are HTML
/// (<c>parse_mode=HTML</c>); every button sits on its own row.
/// </summary>
public interface ITelegramBot
{
    /// <summary>False while <c>Telegram:BotToken</c> is not set: nothing can be sent.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends a message; throws when Telegram refuses, so the outbox tries again later.</summary>
    Task SendAsync(long chatId, string html, IReadOnlyList<TelegramButton> buttons, CancellationToken cancellationToken);

    /// <summary>Sends a message at once, best effort (interactive answers); false when it failed (logged).</summary>
    Task<bool> TrySendAsync(long chatId, string html, IReadOnlyList<TelegramButton> buttons, CancellationToken cancellationToken);

    /// <summary>Shows a short notice on a pressed button, best effort.</summary>
    Task TryAnswerButtonAsync(string callbackId, string text, CancellationToken cancellationToken);

    /// <summary>The bot's @username (cached), or null when it is not configured or Telegram cannot be reached.</summary>
    Task<string?> GetUsernameAsync(CancellationToken cancellationToken);

    /// <summary>Points Telegram's webhook at this API when it points elsewhere; returns <c>ok</c>, <c>set</c>, <c>no_url</c> or <c>failed</c>.</summary>
    Task<string> EnsureWebhookAsync(CancellationToken cancellationToken);
}

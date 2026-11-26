using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Application;
using Notifications.Application.Sending;

namespace Notifications.Infrastructure.Telegram;

/// <summary>
/// <see cref="ITelegramBot"/> over the Telegram Bot API (<c>https://api.telegram.org/bot&lt;token&gt;/&lt;method&gt;</c>).
/// The token never appears in a log or an exception message.
/// </summary>
internal sealed partial class TelegramBotClient(
    HttpClient http,
    IOptions<TelegramOptions> options,
    IMemoryCache cache,
    ILogger<TelegramBotClient> logger) : ITelegramBot
{
    private const string UsernameCacheKey = "notifications.telegram.username";
    private static readonly string[] AllowedUpdates = ["message", "callback_query", "my_chat_member"];

    private string Token => options.Value.BotToken;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Token);

    public async Task SendAsync(long chatId, string html, IReadOnlyList<TelegramButton> buttons, CancellationToken cancellationToken)
    {
        var payload = new JsonObject
        {
            ["chat_id"] = chatId,
            ["text"] = html,
            ["parse_mode"] = "HTML",
            ["link_preview_options"] = new JsonObject { ["is_disabled"] = true },
        };
        if (buttons.Count > 0)
        {
            var rows = new JsonArray();
            foreach (var button in buttons)
                rows.Add(new JsonArray(new JsonObject { ["text"] = button.Text, ["callback_data"] = button.Data }));
            payload["reply_markup"] = new JsonObject { ["inline_keyboard"] = rows };
        }

        await CallAsync("sendMessage", payload, cancellationToken);
    }

    public async Task<bool> TrySendAsync(long chatId, string html, IReadOnlyList<TelegramButton> buttons, CancellationToken cancellationToken)
    {
        try
        {
            await SendAsync(chatId, html, buttons, cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException or JsonException)
        {
            LogCallFailed(logger, "sendMessage", exception.Message);
            return false;
        }
    }

    public async Task TryAnswerButtonAsync(string callbackId, string text, CancellationToken cancellationToken)
    {
        try
        {
            await CallAsync("answerCallbackQuery", new JsonObject { ["callback_query_id"] = callbackId, ["text"] = text }, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException or JsonException)
        {
            LogCallFailed(logger, "answerCallbackQuery", exception.Message);
        }
    }

    public async Task<string?> GetUsernameAsync(CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            return null;
        if (cache.TryGetValue(UsernameCacheKey, out string? cached))
            return cached;

        try
        {
            var result = await CallAsync("getMe", new JsonObject(), cancellationToken);
            var username = result?["username"]?.GetValue<string>();
            cache.Set(UsernameCacheKey, username, TimeSpan.FromHours(1));
            return username;
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException or JsonException)
        {
            LogCallFailed(logger, "getMe", exception.Message);
            return null;
        }
    }

    public async Task<string> EnsureWebhookAsync(CancellationToken cancellationToken)
    {
        var url = options.Value.WebhookUrl;
        if (string.IsNullOrWhiteSpace(url))
            return "no_url";

        try
        {
            var info = await CallAsync("getWebhookInfo", new JsonObject(), cancellationToken);
            if (info?["url"]?.GetValue<string>() == url)
                return "ok";

            await CallAsync(
                "setWebhook",
                new JsonObject
                {
                    ["url"] = url,
                    ["secret_token"] = TelegramWebhookSecret.For(Token),
                    ["allowed_updates"] = new JsonArray([.. AllowedUpdates.Select(u => JsonValue.Create(u))]),
                },
                cancellationToken);
            return "set";
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException or TaskCanceledException or JsonException)
        {
            LogCallFailed(logger, "setWebhook", exception.Message);
            return "failed";
        }
    }

    /// <summary>Calls a method and returns its <c>result</c>; throws when Telegram answers <c>ok: false</c>.</summary>
    private async Task<JsonNode?> CallAsync(string method, JsonObject payload, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Telegram:BotToken is not set.");

        using var response = await http.PostAsJsonAsync($"bot{Token}/{method}", payload, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        if (body?["ok"]?.GetValue<bool>() != true)
        {
            var description = body?["description"]?.GetValue<string>() ?? $"HTTP {(int)response.StatusCode}";
            throw new InvalidOperationException($"Telegram {method} failed: {description}");
        }

        return body["result"];
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Telegram {Method} failed: {Reason}")]
    private static partial void LogCallFailed(ILogger logger, string method, string reason);
}

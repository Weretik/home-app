using System.Collections.Concurrent;
using Notifications.Application;
using Notifications.Application.Sending;

namespace IntegrationTests.Api.Notifications;

/// <summary>Telegram for the tests: records what the API sends instead of calling the Bot API.</summary>
internal sealed class FakeTelegramBot : ITelegramBot
{
    public ConcurrentQueue<(long ChatId, string Text, IReadOnlyList<TelegramButton> Buttons)> Sent { get; } = new();

    public bool IsConfigured => true;

    public Task SendAsync(long chatId, string html, IReadOnlyList<TelegramButton> buttons, CancellationToken cancellationToken)
    {
        Sent.Enqueue((chatId, html, buttons));
        return Task.CompletedTask;
    }

    public async Task<bool> TrySendAsync(long chatId, string html, IReadOnlyList<TelegramButton> buttons, CancellationToken cancellationToken)
    {
        await SendAsync(chatId, html, buttons, cancellationToken);
        return true;
    }

    public Task TryAnswerButtonAsync(string callbackId, string text, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<string?> GetUsernameAsync(CancellationToken cancellationToken) => Task.FromResult<string?>("home_test_bot");

    public Task<string> EnsureWebhookAsync(CancellationToken cancellationToken) => Task.FromResult("ok");

    public IReadOnlyList<string> TextsTo(long chatId) => [.. Sent.Where(m => m.ChatId == chatId).Select(m => m.Text)];
}

using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure.Telegram;

namespace UnitTests.Notifications.Telegram;

public sealed class TelegramBotClientTests
{
    // A real BotFather token has a colon; without a leading slash "bot123:ABC/getMe" parses as an absolute uri with
    // the scheme "bot123", and HttpClient refused it (the «Телеграм» page answered 500 in production).
    private const string RealShapedToken = "123456789:AAH-real_Token";

    [Fact]
    public async Task A_Token_WithAColon_StaysInThePath_OfTheTelegramHost()
    {
        var handler = new StubHandler();
        var client = Create(handler);

        var username = await client.GetUsernameAsync(CancellationToken.None);

        Assert.Equal("home_bot", username);
        Assert.Equal(
            $"https://api.telegram.org/bot{RealShapedToken}/getMe",
            handler.LastUri?.ToString());
    }

    private static TelegramBotClient Create(StubHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://api.telegram.org/") },
        Options.Create(new TelegramOptions { BotToken = RealShapedToken }),
        new MemoryCache(new MemoryCacheOptions()),
        NullLogger<TelegramBotClient>.Instance);

    private sealed class StubHandler : HttpMessageHandler
    {
        public Uri? LastUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"ok":true,"result":{"username":"home_bot"}}""", Encoding.UTF8, "application/json"),
            });
        }
    }
}

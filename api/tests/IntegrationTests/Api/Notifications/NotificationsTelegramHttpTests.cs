using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application;
using Notifications.Infrastructure.Telegram;

namespace IntegrationTests.Api.Notifications;

/// <summary>
/// The real Telegram client of the API, with a token in the BotFather format (digits, a colon, letters) and only the
/// network replaced: the «Телеграм» page, the tick and a test message must reach <c>https://api.telegram.org/bot&lt;token&gt;/…</c>.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class NotificationsTelegramHttpTests(ApiFactory api)
{
    private const string Token = "7712345678:AAHk-real_Format_Token0123456789abcde";
    private const long FamilyChat = -9_002;

    [SkippableFact]
    public async Task The_RealClient_CallsTheBotApi_AtThePathWithTheToken()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        var network = new TelegramNetworkStub();
        await using var host = api.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Telegram:BotToken", Token);
            builder.UseSetting("Notifications:ApiUrl", "https://api.example.test");
            builder.ConfigureTestServices(services =>
                services.AddHttpClient<ITelegramBot, TelegramBotClient>().ConfigurePrimaryHttpMessageHandler(() => network));
        });
        using var client = api.SignedIn(host.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") }));

        var statusResponse = await client.GetAsync("/api/notifications/status");
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var status = await statusResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("https://t.me/home_real_bot", status.GetProperty("botLink").GetString());

        using var tick = new HttpRequestMessage(HttpMethod.Post, "/internal/notifications/tick");
        tick.Headers.Add("X-Scheduler-Key", ApiFactory.TickKey);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(tick)).StatusCode);

        using var joined = new HttpRequestMessage(HttpMethod.Post, "/api/notifications/telegram/webhook")
        {
            Content = JsonContent.Create(new
            {
                update_id = 1,
                my_chat_member = new { chat = new { id = FamilyChat, type = "group", title = "Семья" }, new_chat_member = new { status = "member" } },
            }),
        };
        joined.Headers.Add(TelegramWebhookSecret.HeaderName, TelegramWebhookSecret.For(Token));
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(joined)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/notifications/test", new { audience = "Family" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/notifications/chats/Family")).StatusCode);

        var methods = network.Requests.Select(uri => uri.AbsolutePath.Split('/')[^1]).ToHashSet();
        Assert.Superset(new HashSet<string> { "getMe", "getWebhookInfo", "setWebhook", "sendMessage" }, methods);
        Assert.All(network.Requests, uri =>
            Assert.StartsWith($"https://api.telegram.org/bot{Token}/", uri.ToString(), StringComparison.Ordinal));
    }
}

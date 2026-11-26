using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Api.Todos;
using IntegrationTests.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application;
using Notifications.Infrastructure.Telegram;

namespace IntegrationTests.Api.Notifications;

/// <summary>
/// The bot with a token and a recording Telegram, at 08:30 in Kyiv on 2 December 2026: linking from Telegram, the
/// morning tick planning once, sending through the outbox, and «✅» closing a task.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class NotificationsBotApiTests(ApiFactory api)
{
    private const string Token = "123456:integration-token";
    private const long MashaChat = 5_001;
    private const long FamilyChat = -9_001;

    [SkippableFact]
    public async Task Chats_AreLinkedFromTelegram_TheMorningGoesOnce_AndDone_ClosesTheTask()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        var bot = new FakeTelegramBot();
        await using var host = api.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Telegram:BotToken", Token);
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ITelegramBot>(bot);
                services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 12, 2, 6, 30, 0, TimeSpan.Zero)));
            });
        });
        using var client = host.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        Assert.Equal(HttpStatusCode.Unauthorized, (await WebhookAsync(client, new { update_id = 1 }, "wrong")).StatusCode);

        await WebhookAsync(client, new { update_id = 2, message = new { text = "/start", chat = new { id = MashaChat, type = "private", first_name = "Маша" } } });
        await WebhookAsync(client, new { update_id = 3, callback_query = new { id = "c1", data = "me:Maria", message = new { chat = new { id = MashaChat, type = "private" } } } });
        await WebhookAsync(client, new { update_id = 4, my_chat_member = new { chat = new { id = FamilyChat, type = "group", title = "Семья" }, new_chat_member = new { status = "member" } } });

        var status = await client.GetFromJsonAsync<JsonElement>("/api/notifications/status");
        var linked = status.GetProperty("chats").EnumerateArray().Where(c => c.GetProperty("linked").GetBoolean()).Select(c => c.GetProperty("audience").GetString());
        Assert.True(status.GetProperty("configured").GetBoolean());
        Assert.Equal("https://t.me/home_test_bot", status.GetProperty("botLink").GetString());
        Assert.Equal(["Maria", "Family"], linked);

        var taskId = await TodoRequests.CreateTaskAsync(client, "Бот: купить батарейки", new { dueDate = "2026-12-02" });

        Assert.True(await PlannedAsync(client) >= 2);
        Assert.Equal(0, await PlannedAsync(client));
        await TickAsync(client, "/internal/outbox/tick");

        Assert.Contains(bot.TextsTo(MashaChat), t => t.Contains("Доброе утро") && t.Contains("Бот: купить батарейки"));
        Assert.Contains(bot.TextsTo(FamilyChat), t => t.Contains("Дела семьи"));
        Assert.Contains(bot.Sent, m => m.Buttons.Any(b => b.Data == "done:" + taskId.ToString("N")));

        await WebhookAsync(client, new { update_id = 5, callback_query = new { id = "c2", data = "done:" + taskId.ToString("N"), message = new { chat = new { id = MashaChat, type = "private" } } } });
        Assert.Equal("Done", (await TodoRequests.GetTaskAsync(client, taskId)).GetProperty("status").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/notifications/test", new { audience = "Family" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/notifications/chats/Maria")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/notifications/chats/Family")).StatusCode);
    }

    private static async Task<HttpResponseMessage> WebhookAsync(HttpClient client, object update, string? secret = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/notifications/telegram/webhook") { Content = JsonContent.Create(update) };
        request.Headers.Add(TelegramWebhookSecret.HeaderName, secret ?? TelegramWebhookSecret.For(Token));
        return await client.SendAsync(request);
    }

    private static async Task<JsonElement> TickAsync(HttpClient client, string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-Scheduler-Key", ApiFactory.TickKey);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<int> PlannedAsync(HttpClient client) =>
        (await TickAsync(client, "/internal/notifications/tick")).GetProperty("planned").GetInt32();
}

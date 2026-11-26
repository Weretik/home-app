using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Notifications;

/// <summary>The site endpoints and the machine guards with no bot token: the default state of a new install.</summary>
[Collection(ApiCollection.Name)]
public sealed class NotificationsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Status_ListsEveryAudience_AndTheSchedule_WithoutAToken()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var status = await client.GetFromJsonAsync<JsonElement>("/api/notifications/status");

        Assert.False(status.GetProperty("configured").GetBoolean());
        Assert.Equal(JsonValueKind.Null, status.GetProperty("botLink").ValueKind);
        Assert.Equal(["Vitaliy", "Maria", "Family"], status.GetProperty("chats").EnumerateArray().Select(c => c.GetProperty("audience").GetString()));
        Assert.Equal(6, status.GetProperty("schedule").GetArrayLength());
    }

    [SkippableFact]
    public async Task Unlinking_AChatThatIsNotLinked_Is404_AndATestWithoutAToken_Is409()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var unlink = await client.DeleteAsync("/api/notifications/chats/Vitaliy");
        var test = await client.PostAsJsonAsync("/api/notifications/test", new { audience = "Vitaliy" });
        var wrong = await client.PostAsJsonAsync("/api/notifications/test", new { audience = "Nobody" });

        Assert.Equal(HttpStatusCode.NotFound, unlink.StatusCode);
        Assert.Equal("notifications.chat_not_linked", await CodeAsync(unlink));
        Assert.Equal(HttpStatusCode.Conflict, test.StatusCode);
        Assert.Equal("notifications.not_configured", await CodeAsync(test));
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
    }

    [SkippableFact]
    public async Task The_Webhook_Is404_WithoutAToken_AndTheTick_NeedsTheSchedulerKey()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var webhook = await client.PostAsJsonAsync("/api/notifications/telegram/webhook", new { update_id = 1 });
        var noKey = await client.PostAsync("/internal/notifications/tick", null);
        using var keyed = new HttpRequestMessage(HttpMethod.Post, "/internal/notifications/tick");
        keyed.Headers.Add("X-Scheduler-Key", ApiFactory.TickKey);
        var tick = await client.SendAsync(keyed);

        Assert.Equal(HttpStatusCode.NotFound, webhook.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, noKey.StatusCode);
        Assert.Equal(HttpStatusCode.OK, tick.StatusCode);
        Assert.False((await tick.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("configured").GetBoolean());
    }

    private static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();
}

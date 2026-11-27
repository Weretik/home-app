using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using IntegrationTests.Api.Todos;
using IntegrationTests.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Push;

namespace IntegrationTests.Api.Notifications.Push;

/// <summary>
/// Web Push (spec notifications/002): subscriptions per person, the tick planning push next to Telegram, the outbox
/// sending, gone devices removed, and push without keys answering "not configured".
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class NotificationsPushApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Without_Keys_PushIsNotConfigured_AndCannotBeSaved()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.SignedIn(api.CreateAnonymousClient());

        var status = await client.GetFromJsonAsync<JsonElement>("/api/notifications/push");
        var save = await client.PutAsJsonAsync("/api/notifications/push/subscriptions", Subscription("https://push.example/not-configured"));

        Assert.False(status.GetProperty("configured").GetBoolean());
        Assert.Equal(JsonValueKind.Null, status.GetProperty("publicKey").ValueKind);
        Assert.Equal(HttpStatusCode.Conflict, save.StatusCode);
        Assert.Equal("notifications.push_not_configured", (await save.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
    }

    [SkippableFact]
    public async Task Devices_ArePerPerson_TheMorningIsPushedOnce_AndGoneDevicesAreRemoved()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        var push = new FakeWebPush();
        await using var host = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IWebPush>(push);
            // 08:30 in Kyiv on 9 December 2026: a day no other test ticks on.
            services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2026, 12, 9, 6, 30, 0, TimeSpan.Zero)));
        }));
        using var vitaliy = api.SignedIn(host.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") }));
        using var masha = host.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        masha.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await api.SignInAsync(ApiFactory.Masha));
        var phone = $"https://push.example/phone-{Guid.NewGuid():N}";

        // R-102: an upsert by endpoint.
        var first = await SaveAsync(vitaliy, phone);
        var again = await SaveAsync(vitaliy, phone, "Android · Chrome");
        Assert.Equal(first.GetProperty("id").GetGuid(), again.GetProperty("id").GetGuid());
        Assert.Equal("Android · Chrome", again.GetProperty("device").GetString());
        var invalid = await vitaliy.PutAsJsonAsync("/api/notifications/push/subscriptions", Subscription("http://push.example/plain"));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        // R-103: each person sees and removes only their own devices.
        var status = await vitaliy.GetFromJsonAsync<JsonElement>("/api/notifications/push");
        Assert.True(status.GetProperty("configured").GetBoolean());
        Assert.True(status.GetProperty("canSubscribe").GetBoolean());
        Assert.Equal("BFakePublicKeyForTheIntegrationTests", status.GetProperty("publicKey").GetString());
        Assert.Single(status.GetProperty("devices").EnumerateArray(), d => d.GetProperty("endpoint").GetString() == phone);
        var mashas = await masha.GetFromJsonAsync<JsonElement>("/api/notifications/push");
        Assert.DoesNotContain(mashas.GetProperty("devices").EnumerateArray(), d => d.GetProperty("endpoint").GetString() == phone);
        Assert.Equal(HttpStatusCode.NotFound, (await masha.DeleteAsync($"/api/notifications/push/subscriptions/{first.GetProperty("id").GetGuid()}")).StatusCode);

        // R-105, R-106: the morning digest is planned as push without a Telegram token, once.
        await TodoRequests.CreateTaskAsync(vitaliy, "Push: забрати посилку", new { assignee = "Vitaliy", dueDate = "2026-12-09" });
        Assert.True(await TickAsync(vitaliy, "/internal/notifications/tick") >= 1);
        Assert.Equal(0, await TickAsync(vitaliy, "/internal/notifications/tick"));
        await TickAsync(vitaliy, "/internal/outbox/tick");
        Assert.Contains(push.Sent, m => m.Endpoint == phone && m.Payload.Title == "Справи на сьогодні"
            && m.Payload.Body.Contains("Push: забрати посилку", StringComparison.Ordinal) && m.Payload.Url == "/todos");
        var deliveries = (await vitaliy.GetFromJsonAsync<JsonElement>("/api/notifications/status")).GetProperty("deliveries");
        Assert.Contains(deliveries.EnumerateArray(), d => d.GetProperty("channel").GetString() == "Push"
            && d.GetProperty("key").GetString() == "push:TodosMorning:2026-12-09:Vitaliy");

        // R-108, R-109: the test push reaches the phone; a gone device is removed.
        var gone = await SaveAsync(vitaliy, $"https://push.example/gone-{Guid.NewGuid():N}");
        var test = await vitaliy.PostAsync("/api/notifications/push/test", null);
        Assert.Equal(HttpStatusCode.OK, test.StatusCode);
        Assert.True((await test.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("delivered").GetInt32() >= 1);
        var after = await vitaliy.GetFromJsonAsync<JsonElement>("/api/notifications/push");
        Assert.DoesNotContain(after.GetProperty("devices").EnumerateArray(), d => d.GetProperty("id").GetGuid() == gone.GetProperty("id").GetGuid());

        // Cleanup keeps the other tests' push maps empty.
        foreach (var device in after.GetProperty("devices").EnumerateArray())
            Assert.Equal(HttpStatusCode.NoContent, (await vitaliy.DeleteAsync($"/api/notifications/push/subscriptions/{device.GetProperty("id").GetGuid()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await vitaliy.PostAsync("/api/notifications/push/test", null)).StatusCode);
    }

    private static object Subscription(string endpoint, string? device = null)
    {
        using var key = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var q = key.ExportParameters(false).Q;
        return new
        {
            endpoint,
            p256dh = Base64Url([4, .. q.X!, .. q.Y!]),
            auth = Base64Url(RandomNumberGenerator.GetBytes(16)),
            device,
        };
    }

    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static async Task<JsonElement> SaveAsync(HttpClient client, string endpoint, string? device = null)
    {
        var response = await client.PutAsJsonAsync("/api/notifications/push/subscriptions", Subscription(endpoint, device));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<int> TickAsync(HttpClient client, string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-Scheduler-Key", ApiFactory.TickKey);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.TryGetProperty("planned", out var planned) ? planned.GetInt32() : 0;
    }
}

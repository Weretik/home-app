using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Api.Todos;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Gifts.GiftsApi;

namespace IntegrationTests.Api.Gifts;

/// <summary>Spec gifts/002 end to end through the outbox: a planned gift becomes the giver's to-do, done → gifted.</summary>
[Collection(ApiCollection.Name)]
public sealed class GiftTodoFlowApiTests(ApiFactory api)
{
    private const string Jewellery = "b5691e11-6405-5690-bba9-43e92fef7366";

    [SkippableFact]
    public async Task A_Planned_Gift_BecomesATodo_AndDoingIt_MovesTheGiftToGifted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var name = "Браслет " + Guid.NewGuid().ToString("N")[..6];

        var created = await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Maria",
            name,
            categoryId = Jewellery,
            priority = "High",
            variants = Array.Empty<object>(),
            giver = "Vitaliy",
            buyBy = "2040-12-20",
        });
        var giftId = await IdOfAsync(created);
        Assert.Equal("Vitaliy", (await ItemAsync(client, giftId)).GetProperty("giver").GetString());

        await OutboxTickAsync(client);
        var task = await GiftTaskAsync(client, giftId, name);
        Assert.Equal($"🎁 Купить подарок: {name} (для Маши)", task.GetProperty("title").GetString());
        Assert.Equal("Vitaliy", task.GetProperty("assignee").GetString());
        Assert.Equal("2040-12-20", task.GetProperty("dueDate").GetString());

        var taskId = task.GetProperty("id").GetGuid();
        var version = (await TodoRequests.GetTaskAsync(client, taskId)).GetProperty("version").GetUInt32();
        (await TodoRequests.SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{taskId}/status", version, new { status = "Done" })).EnsureSuccessStatusCode();
        await OutboxTickAsync(client);

        Assert.Equal("Gifted", (await ItemAsync(client, giftId, archived: true)).GetProperty("status").GetString());
    }

    private static async Task OutboxTickAsync(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/internal/outbox/tick");
        request.Headers.Add("X-Scheduler-Key", ApiFactory.TickKey);
        (await client.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private static async Task<JsonElement> GiftTaskAsync(HttpClient client, Guid giftId, string name)
    {
        var list = await client.GetFromJsonAsync<JsonElement>($"/api/todos/tasks?search={Uri.EscapeDataString(name)}");
        return list.GetProperty("items").EnumerateArray()
            .Single(t => t.GetProperty("sourceKind").GetString() == "gift" && t.GetProperty("sourceId").GetGuid() == giftId);
    }
}

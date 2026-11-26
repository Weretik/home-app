using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Expenses.Budget;

/// <summary>
/// The «Прогноз» tab end to end: planning expenses for a month, editing and dropping them, recording one as a real
/// expense, and the forecast in the month summary. Uses its own far-future month.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesForecastApiTests(ApiFactory api)
{
    private static readonly Guid Groceries = new("86c6ddca-a991-5d9b-97b9-468b90f50c4c");
    private static readonly Guid Gifts = new("02d964e6-9360-5e7f-9685-6133d03031ef");

    [SkippableFact]
    public async Task A_Plan_IsAdded_Changed_Recorded_AndDropped()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var giftsBody = new { year = 2050, month = 5, name = "  Подарки  ", categoryId = Gifts, payer = "Shared", amount = 3_000m };
        var key = Guid.NewGuid().ToString();

        var gifts = await PostAsync(client, "/api/expenses/planned", key, giftsBody);
        Assert.Equal(HttpStatusCode.Created, gifts.StatusCode);
        var giftsId = (await gifts.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.EndsWith($"/api/expenses/planned/{giftsId}", gifts.Headers.Location!.ToString());
        var repeat = await PostAsync(client, "/api/expenses/planned", key, giftsBody);
        Assert.Equal(giftsId, (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, (await PostAsync(client, "/api/expenses/planned", key, giftsBody with { amount = 1m })).StatusCode);

        var tree = await PostAsync(client, "/api/expenses/planned", Guid.NewGuid().ToString(),
            new { year = 2050, month = 5, name = "Ёлка", categoryId = Groceries, payer = "Maria", amount = 1_000m });
        Assert.Equal(HttpStatusCode.Created, tree.StatusCode);
        var treeId = (await tree.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var list = await client.GetFromJsonAsync<JsonElement>("/api/expenses/planned?year=2050&month=5");
        Assert.Equal(4_000m, list.GetProperty("totalAmount").GetDecimal());
        var items = list.GetProperty("items");
        Assert.Equal([treeId, giftsId], items.EnumerateArray().Select(i => i.GetProperty("id").GetGuid()));
        var giftsItem = items[1];
        Assert.Equal(("Подарки", Gifts, "Подарки", "Shared", 3_000m, 2050, 5), (
            giftsItem.GetProperty("name").GetString(),
            giftsItem.GetProperty("categoryId").GetGuid(),
            giftsItem.GetProperty("categoryName").GetString(),
            giftsItem.GetProperty("payer").GetString(),
            giftsItem.GetProperty("amount").GetDecimal(),
            giftsItem.GetProperty("year").GetInt32(),
            giftsItem.GetProperty("month").GetInt32()));

        var summary = await SummaryAsync(client);
        var forecast = summary.GetProperty("forecast");
        Assert.Equal((4_000m, 4_000m, -4_000m, 46_000m), (
            forecast.GetProperty("planned").GetDecimal(),
            forecast.GetProperty("expectedExpenses").GetDecimal(),
            forecast.GetProperty("expectedResult").GetDecimal(),
            forecast.GetProperty("expectedTotalLeft").GetDecimal()));
        Assert.Equal(1_000m, Limit(summary, "Еда и быт").GetProperty("planned").GetDecimal());
        Assert.Equal(3_000m, Limit(summary, "Другое").GetProperty("planned").GetDecimal());
        Assert.Equal(0m, Limit(summary, "Еда и быт").GetProperty("spent").GetDecimal());

        var treeVersion = items[0].GetProperty("version").GetUInt32();
        var changed = await SendAsync(client, HttpMethod.Put, $"/api/expenses/planned/{treeId}", treeVersion,
            new { year = 2050, month = 5, name = "Ёлка", categoryId = Groceries, payer = "Maria", amount = 1_500m });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        var stale = await SendAsync(client, HttpMethod.Put, $"/api/expenses/planned/{treeId}", treeVersion,
            new { year = 2050, month = 5, name = "Ёлка", categoryId = Groceries, payer = "Maria", amount = 1m });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        var staleRecord = await SendAsync(client, HttpMethod.Post, $"/api/expenses/planned/{treeId}/record", treeVersion,
            new { date = "2050-05-10", paymentMethod = "Card", comment = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, staleRecord.StatusCode);

        var current = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/planned?year=2050&month=5"))
            .GetProperty("items")[0];
        Assert.Equal(1_500m, current.GetProperty("amount").GetDecimal());
        var recorded = await SendAsync(client, HttpMethod.Post, $"/api/expenses/planned/{treeId}/record",
            current.GetProperty("version").GetUInt32(), new { date = "2050-05-10", paymentMethod = "Cash", comment = "рынок" });
        Assert.Equal(HttpStatusCode.Created, recorded.StatusCode);
        var expenseId = (await recorded.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.EndsWith($"/api/expenses/{expenseId}", recorded.Headers.Location!.ToString());
        var again = await SendAsync(client, HttpMethod.Post, $"/api/expenses/planned/{treeId}/record",
            current.GetProperty("version").GetUInt32(), new { date = "2050-05-10", paymentMethod = "Cash", comment = "рынок" });
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);

        var expenses = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2050&month=5");
        Assert.Equal(1, expenses.GetProperty("totalCount").GetInt32());
        var expense = expenses.GetProperty("items")[0];
        Assert.Equal((expenseId, "2050-05-10", "Maria", Groceries, 1_500m, "Cash", "рынок", false), (
            expense.GetProperty("id").GetGuid(),
            expense.GetProperty("date").GetString(),
            expense.GetProperty("payer").GetString(),
            expense.GetProperty("categoryId").GetGuid(),
            expense.GetProperty("amount").GetDecimal(),
            expense.GetProperty("paymentMethod").GetString(),
            expense.GetProperty("comment").GetString(),
            expense.GetProperty("outsideLimits").GetBoolean()));

        summary = await SummaryAsync(client);
        Assert.Equal(1_500m, summary.GetProperty("expenses").GetDecimal());
        Assert.Equal(3_000m, summary.GetProperty("forecast").GetProperty("planned").GetDecimal());
        Assert.Equal(4_500m, summary.GetProperty("forecast").GetProperty("expectedExpenses").GetDecimal());
        Assert.Equal(1_500m, Limit(summary, "Еда и быт").GetProperty("spent").GetDecimal());
        Assert.Equal(0m, Limit(summary, "Еда и быт").GetProperty("planned").GetDecimal());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/expenses/planned/{giftsId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/expenses/planned/{giftsId}")).StatusCode);
        var empty = await client.GetFromJsonAsync<JsonElement>("/api/expenses/planned?year=2050&month=5");
        Assert.Equal(0, empty.GetProperty("items").GetArrayLength());
        Assert.Equal(0m, empty.GetProperty("totalAmount").GetDecimal());
    }

    [SkippableFact]
    public async Task An_Invalid_Plan_IsRefused()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var body = new { year = 2050, month = 6, name = "План", categoryId = Gifts, payer = "Shared", amount = 10m };

        var unknownCategory = await PostAsync(client, "/api/expenses/planned", Guid.NewGuid().ToString(), body with { categoryId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.NotFound, unknownCategory.StatusCode);
        Assert.Equal("expenses.category_not_found", (await unknownCategory.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, (await PostAsync(client, "/api/expenses/planned", Guid.NewGuid().ToString(), body with { month = 13 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostAsync(client, "/api/expenses/planned", Guid.NewGuid().ToString(), body with { name = " " })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await PostAsync(client, "/api/expenses/planned", Guid.NewGuid().ToString(), body with { amount = 0.001m })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/expenses/planned", body)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/expenses/planned?year=2050&month=0")).StatusCode);

        var missing = Guid.NewGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(client, HttpMethod.Put, $"/api/expenses/planned/{missing}", 5, body)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/api/expenses/planned/{missing}", body)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(client, HttpMethod.Post, $"/api/expenses/planned/{missing}/record", 5,
            new { date = "2050-06-01", paymentMethod = "Card", comment = (string?)null })).StatusCode);
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/expenses/planned?year=2050&month=6")).GetProperty("items").GetArrayLength());
    }

    private static Task<JsonElement> SummaryAsync(HttpClient client) =>
        client.GetFromJsonAsync<JsonElement>("/api/expenses/summary?year=2050&month=5");

    private static JsonElement Limit(JsonElement summary, string name) =>
        summary.GetProperty("limits").EnumerateArray().Single(l => l.GetProperty("name").GetString() == name);

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, string key, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", key);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string url, uint version, object body)
    {
        using var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }
}

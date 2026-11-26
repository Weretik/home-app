using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Expenses.Entries;

/// <summary>
/// The savings jar «Копилка» end to end: month sums, the months overview, edits, deletes, repeats and stale versions
/// (SDD 002). Each test uses its own year.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesSavingsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Savings_Movements_AddUp_ByMonth()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        await PostAsync(client, "/api/expenses/savings", Guid.NewGuid().ToString(),
            new { date = "2043-01-10", person = "Vitaliy", amount = 1_000m, comment = "отложили" });
        await PostAsync(client, "/api/expenses/savings", Guid.NewGuid().ToString(),
            new { date = "2043-02-03", person = "Vitaliy", amount = 300m, comment = (string?)null });
        var taken = await PostAsync(client, "/api/expenses/savings", Guid.NewGuid().ToString(),
            new { date = "2043-02-05", person = "Vitaliy", amount = -120.50m, comment = "кафе" });
        var zero = await PostAsync(client, "/api/expenses/savings", Guid.NewGuid().ToString(),
            new { date = "2043-02-06", person = "Vitaliy", amount = 0m, comment = (string?)null });
        Assert.Equal(HttpStatusCode.Created, taken.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, zero.StatusCode);

        var february = await client.GetFromJsonAsync<JsonElement>("/api/expenses/savings?year=2043&month=2");
        Assert.Equal(2, february.GetProperty("totalCount").GetInt32());
        Assert.Equal(300m, february.GetProperty("deposits").GetDecimal());
        Assert.Equal(120.50m, february.GetProperty("withdrawals").GetDecimal());
        Assert.Equal(179.50m, february.GetProperty("balanceAtEnd").GetDecimal() - february.GetProperty("balanceAtStart").GetDecimal());
        var withdrawal = february.GetProperty("items")[0];
        Assert.Equal("Withdrawal", withdrawal.GetProperty("direction").GetString());
        Assert.Equal(-120.50m, withdrawal.GetProperty("amount").GetDecimal());

        var overview = await client.GetFromJsonAsync<JsonElement>("/api/expenses/savings/months");
        var months = overview.GetProperty("months").EnumerateArray()
            .Where(m => m.GetProperty("year").GetInt32() == 2043)
            .ToDictionary(m => m.GetProperty("month").GetInt32());
        Assert.Equal(1_000m, months[1].GetProperty("deposits").GetDecimal());
        Assert.Equal(120.50m, months[2].GetProperty("withdrawals").GetDecimal());
        Assert.Equal(179.50m, months[2].GetProperty("balanceAtEnd").GetDecimal() - months[1].GetProperty("balanceAtEnd").GetDecimal());

        var id = withdrawal.GetProperty("id").GetGuid();
        var changed = await PutAsync(client, $"/api/expenses/savings/{id}", withdrawal.GetProperty("version").GetUInt32(),
            new { date = "2043-02-05", person = "Vitaliy", amount = -20.50m, comment = "кафе" });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        var after = await client.GetFromJsonAsync<JsonElement>("/api/expenses/savings?year=2043&month=2");
        Assert.Equal(20.50m, after.GetProperty("withdrawals").GetDecimal());
    }

    [SkippableFact]
    public async Task A_Movement_IsRepeatedOnce_RefusedWhenStale_AndDeleted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var key = Guid.NewGuid().ToString();
        var body = new { date = "2052-03-10", person = "Maria", amount = 500m, comment = "отложили" };

        var first = await PostAsync(client, "/api/expenses/savings", key, body);
        var repeat = await PostAsync(client, "/api/expenses/savings", key, body);
        var other = await PostAsync(client, "/api/expenses/savings", key, body with { amount = -500m });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var id = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(id, (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);

        var march = await client.GetFromJsonAsync<JsonElement>("/api/expenses/savings?year=2052&month=3");
        Assert.Equal(1, march.GetProperty("totalCount").GetInt32());
        var version = march.GetProperty("items")[0].GetProperty("version").GetUInt32();

        Assert.Equal(HttpStatusCode.NoContent, (await PutAsync(client, $"/api/expenses/savings/{id}", version, body with { amount = 600m })).StatusCode);
        var stale = await PutAsync(client, $"/api/expenses/savings/{id}", version, body with { amount = 700m });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("expenses.changed_meanwhile", (await stale.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        var unknown = await PutAsync(client, $"/api/expenses/savings/{Guid.NewGuid()}", version, body);
        Assert.Equal("expenses.savings_movement_not_found", (await unknown.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(600m, (await client.GetFromJsonAsync<JsonElement>("/api/expenses/savings?year=2052&month=3")).GetProperty("deposits").GetDecimal());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/expenses/savings/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/expenses/savings/{id}")).StatusCode);
        var after = await client.GetFromJsonAsync<JsonElement>("/api/expenses/savings?year=2052&month=3");
        Assert.Equal(0, after.GetProperty("totalCount").GetInt32());
        Assert.Equal(0m, after.GetProperty("deposits").GetDecimal());
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, string key, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", key);
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PutAsync(HttpClient client, string url, uint version, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }
}

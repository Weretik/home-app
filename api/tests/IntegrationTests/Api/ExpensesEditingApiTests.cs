using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api;

/// <summary>
/// Adding, changing and deleting expenses, incomes and savings movements end to end. Each test uses its own year, so
/// tests sharing the database do not see each other's rows.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesEditingApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task An_Expense_IsAdded_Changed_AndDeleted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var categories = await client.GetFromJsonAsync<JsonElement>("/api/expenses/categories");
        var groceries = categories[0].GetProperty("id").GetGuid();
        var other = categories[1].GetProperty("id").GetGuid();

        var created = await PostAsync(client, "/api/expenses", Guid.NewGuid().ToString(), new
        {
            date = "2040-10-05",
            payer = "Shared",
            categoryId = groceries,
            amount = 442.50m,
            paymentMethod = "Card",
            comment = "АТБ",
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var item = (await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2040&month=10")).GetProperty("items")[0];
        Assert.Equal(id, item.GetProperty("id").GetGuid());
        var version = item.GetProperty("version").GetUInt32();

        var changed = await PutAsync(client, $"/api/expenses/{id}", version, new
        {
            date = "2040-10-06",
            payer = "Maria",
            categoryId = other,
            amount = 100m,
            paymentMethod = "Cash",
            comment = (string?)null,
        });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        var stale = await PutAsync(client, $"/api/expenses/{id}", version, new
        {
            date = "2040-10-07",
            payer = "Maria",
            categoryId = other,
            amount = 1m,
            paymentMethod = "Cash",
            comment = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("expenses.changed_meanwhile", (await stale.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        var list = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2040&month=10");
        Assert.Equal(100m, list.GetProperty("totalAmount").GetDecimal());
        Assert.Equal("Maria", list.GetProperty("items")[0].GetProperty("payer").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/expenses/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/expenses/{id}")).StatusCode);
        var empty = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2040&month=10");
        Assert.Equal(0, empty.GetProperty("totalCount").GetInt32());
    }

    [SkippableFact]
    public async Task A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var key = Guid.NewGuid().ToString();
        var body = new { date = "2041-03-01", person = "Maria", amount = 25_025m, comment = "зарплата" };

        var first = await PostAsync(client, "/api/expenses/incomes", key, body);
        var repeat = await PostAsync(client, "/api/expenses/incomes", key, body);
        var other = await PostAsync(client, "/api/expenses/incomes", key, body with { amount = 1m });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
        Assert.Equal(
            (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid(),
            (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        var march = await client.GetFromJsonAsync<JsonElement>("/api/expenses/incomes?year=2041&month=3");
        Assert.Equal(1, march.GetProperty("totalCount").GetInt32());
    }

    [SkippableFact]
    public async Task A_Create_WithoutAKey_OrAnEdit_WithoutAVersion_IsAValidationProblem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var body = new { date = "2042-03-01", person = "Vitaliy", amount = 5m, comment = (string?)null };

        var noKey = await client.PostAsJsonAsync("/api/expenses/incomes", body);
        var noVersion = await client.PutAsJsonAsync($"/api/expenses/incomes/{Guid.NewGuid()}", body);

        Assert.Equal(HttpStatusCode.BadRequest, noKey.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noVersion.StatusCode);
    }

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

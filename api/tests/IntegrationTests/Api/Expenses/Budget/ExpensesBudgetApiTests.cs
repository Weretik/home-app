using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Expenses.Budget;

/// <summary>
/// The budget: the seeded limits and settings, and editing them end to end. Tests share the database, so they only
/// count rows they own and put back the settings they change.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesBudgetApiTests(ApiFactory api)
{
    private static readonly (string Name, string Period, decimal Amount, int Categories)[] SeededLimits =
    [
        ("Еда и быт", "Month", 15_000m, 2),
        ("Личные Виталий", "Month", 7_000m, 1),
        ("Личные Маша", "Month", 13_000m, 1),
        ("Кафе, доставка, развлечения", "Month", 3_000m, 1),
        ("Уход и косметика", "Quarter", 3_000m, 1),
        ("Зоотовары", "Quarter", 2_500m, 1),
        ("Другое", "Month", 15_000m, 10),
    ];

    [SkippableFact]
    public async Task The_Migration_Seeds_SevenLimits_InSheetOrder()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var budget = await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget");

        var seededNames = SeededLimits.Select(l => l.Name).ToHashSet();
        var seeded = budget.GetProperty("limits").EnumerateArray()
            .Where(l => seededNames.Contains(l.GetProperty("name").GetString()!))
            .ToList();
        Assert.Equal(SeededLimits.Length, seeded.Count);
        for (var i = 0; i < SeededLimits.Length; i++)
        {
            Assert.Equal(SeededLimits[i].Name, seeded[i].GetProperty("name").GetString());
            Assert.Equal(SeededLimits[i].Period, seeded[i].GetProperty("period").GetString());
            Assert.Equal(SeededLimits[i].Amount, seeded[i].GetProperty("amount").GetDecimal());
            Assert.Equal(SeededLimits[i].Categories, seeded[i].GetProperty("categoryIds").GetArrayLength());
            Assert.NotEqual(0u, seeded[i].GetProperty("version").GetUInt32());
        }

        var categories = await client.GetFromJsonAsync<JsonElement>("/api/expenses/categories");
        var known = categories.EnumerateArray().Select(c => c.GetProperty("id").GetGuid()).ToHashSet();
        Assert.All(
            seeded.SelectMany(l => l.GetProperty("categoryIds").EnumerateArray()),
            id => Assert.Contains(id.GetGuid(), known));
    }

    [SkippableFact]
    public async Task The_Settings_AreSeeded_AndAnEdit_WithAnOldVersion_IsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var before = await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget");
        var version = before.GetProperty("version").GetUInt32();
        Assert.NotEqual(0u, version);
        Assert.Equal(50_000m, before.GetProperty("monthlyLimit").GetDecimal());
        Assert.Equal(100_000m, before.GetProperty("savingsGoal").GetDecimal());
        Assert.Equal("2026-11-01", before.GetProperty("limitsFrom").GetString());

        try
        {
            var changed = await PutAsync(client, "/api/expenses/budget/settings", version, new { monthlyLimit = 42_000.50m, savingsGoal = 0m });
            Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

            var after = await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget");
            Assert.Equal(42_000.50m, after.GetProperty("monthlyLimit").GetDecimal());
            Assert.Equal(0m, after.GetProperty("savingsGoal").GetDecimal());
            // Left out, the day limits start counting stays as it was.
            Assert.Equal("2026-11-01", after.GetProperty("limitsFrom").GetString());
            Assert.NotEqual(version, after.GetProperty("version").GetUInt32());

            var stale = await PutAsync(client, "/api/expenses/budget/settings", version, new { monthlyLimit = 1m, savingsGoal = 1m });
            Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
            Assert.Equal("expenses.changed_meanwhile", (await stale.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

            var noVersion = await client.PutAsJsonAsync("/api/expenses/budget/settings", new { monthlyLimit = 1m, savingsGoal = 1m });
            Assert.Equal(HttpStatusCode.BadRequest, noVersion.StatusCode);

            var current = after.GetProperty("version").GetUInt32();
            var negative = await PutAsync(client, "/api/expenses/budget/settings", current, new { monthlyLimit = -1m, savingsGoal = 0m });
            Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
            var tooPrecise = await PutAsync(client, "/api/expenses/budget/settings", current, new { monthlyLimit = 1.001m, savingsGoal = 0m });
            Assert.Equal(HttpStatusCode.BadRequest, tooPrecise.StatusCode);
        }
        finally
        {
            var current = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("version").GetUInt32();
            var restored = await PutAsync(client, "/api/expenses/budget/settings", current, new { monthlyLimit = 50_000m, savingsGoal = 100_000m });
            Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);
        }
    }

    [SkippableFact]
    public async Task A_Limit_IsAdded_Changed_AndDeleted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var categories = await client.GetFromJsonAsync<JsonElement>("/api/expenses/categories");
        var first = categories[0].GetProperty("id").GetGuid();
        var second = categories[1].GetProperty("id").GetGuid();
        var name = $"Тест {Guid.NewGuid():N}"[..20];

        var created = await PostAsync(client, "/api/expenses/budget/limits", Guid.NewGuid().ToString(),
            new { name, period = "Quarter", amount = 1_234.56m, categoryIds = new[] { first, second } });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.EndsWith($"/api/expenses/budget/limits/{id}", created.Headers.Location!.ToString());

        var limits = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits");
        var limit = Find(limits, id);
        Assert.Equal(id, limits[limits.GetArrayLength() - 1].GetProperty("id").GetGuid());
        Assert.Equal(name, limit.GetProperty("name").GetString());
        Assert.Equal("Quarter", limit.GetProperty("period").GetString());
        Assert.Equal(1_234.56m, limit.GetProperty("amount").GetDecimal());
        Assert.Equal([first, second], limit.GetProperty("categoryIds").EnumerateArray().Select(c => c.GetGuid()));
        var version = limit.GetProperty("version").GetUInt32();

        var changed = await PutAsync(client, $"/api/expenses/budget/limits/{id}", version,
            new { name = $"{name}!", period = "Month", amount = 10m, categoryIds = new[] { second } });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        var stale = await PutAsync(client, $"/api/expenses/budget/limits/{id}", version,
            new { name, period = "Month", amount = 20m, categoryIds = new[] { second } });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("expenses.changed_meanwhile", (await stale.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());

        var updated = Find((await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits"), id);
        Assert.Equal($"{name}!", updated.GetProperty("name").GetString());
        Assert.Equal("Month", updated.GetProperty("period").GetString());
        Assert.Equal(10m, updated.GetProperty("amount").GetDecimal());
        Assert.Equal([second], updated.GetProperty("categoryIds").EnumerateArray().Select(c => c.GetGuid()));

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/expenses/budget/limits/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/expenses/budget/limits/{id}")).StatusCode);
        var remaining = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits");
        Assert.DoesNotContain(remaining.EnumerateArray(), l => l.GetProperty("id").GetGuid() == id);

        var gone = await PutAsync(client, $"/api/expenses/budget/limits/{id}", version,
            new { name, period = "Month", amount = 20m, categoryIds = new[] { second } });
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [SkippableFact]
    public async Task A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var category = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/categories"))[0].GetProperty("id").GetGuid();
        var key = Guid.NewGuid().ToString();
        var body = new { name = "Повтор", period = "Month", amount = 100m, categoryIds = new[] { category } };

        var first = await PostAsync(client, "/api/expenses/budget/limits", key, body);
        var repeat = await PostAsync(client, "/api/expenses/budget/limits", key, body);
        var other = await PostAsync(client, "/api/expenses/budget/limits", key, body with { amount = 200m });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var id = (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        try
        {
            Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
            Assert.Equal(id, (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
            Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        }
        finally
        {
            await client.DeleteAsync($"/api/expenses/budget/limits/{id}");
        }
    }

    [SkippableFact]
    public async Task A_Limit_WithAnUnknownCategory_IsNotFound_AndWithoutCategories_IsAValidationProblem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var before = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits").GetArrayLength();

        var unknown = await PostAsync(client, "/api/expenses/budget/limits", Guid.NewGuid().ToString(),
            new { name = "Нет такой", period = "Month", amount = 100m, categoryIds = new[] { Guid.NewGuid() } });
        var empty = await PostAsync(client, "/api/expenses/budget/limits", Guid.NewGuid().ToString(),
            new { name = "Пусто", period = "Month", amount = 100m, categoryIds = Array.Empty<Guid>() });
        var noKey = await client.PostAsJsonAsync("/api/expenses/budget/limits",
            new { name = "Без ключа", period = "Month", amount = 100m, categoryIds = new[] { Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal("expenses.category_not_found", (await unknown.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noKey.StatusCode);

        var seeded = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits")[0];
        var unknownOnUpdate = await PutAsync(client, $"/api/expenses/budget/limits/{seeded.GetProperty("id").GetGuid()}",
            seeded.GetProperty("version").GetUInt32(),
            new { name = "Нет такой", period = "Month", amount = 100m, categoryIds = new[] { Guid.NewGuid() } });
        Assert.Equal(HttpStatusCode.NotFound, unknownOnUpdate.StatusCode);
        var unchanged = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits")[0];
        Assert.Equal(seeded.GetProperty("name").GetString(), unchanged.GetProperty("name").GetString());
        Assert.Equal(seeded.GetProperty("version").GetUInt32(), unchanged.GetProperty("version").GetUInt32());

        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/expenses/budget/limits/{Guid.NewGuid()}")).StatusCode);
    }

    [SkippableFact]
    public async Task The_MonthSummary_AddsUp_LikeTheSheet()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var groceries = new Guid("86c6ddca-a991-5d9b-97b9-468b90f50c4c");
        var household = new Guid("12b7f2d2-0f0c-5d11-9691-86b3bc919ffe");
        var pets = new Guid("08305dd6-28c2-59a1-9d98-0026828b4eee");

        async Task Add(string url, object body) =>
            Assert.Equal(HttpStatusCode.Created, (await PostAsync(client, url, Guid.NewGuid().ToString(), body)).StatusCode);

        await Add("/api/expenses", new { date = "2047-03-02", payer = "Shared", categoryId = groceries, amount = 1_000m, paymentMethod = "Card", comment = (string?)null });
        await Add("/api/expenses", new { date = "2047-03-03", payer = "Maria", categoryId = household, amount = 500m, paymentMethod = "Card", comment = (string?)null });
        await Add("/api/expenses", new { date = "2047-02-10", payer = "Vitaliy", categoryId = pets, amount = 300m, paymentMethod = "Cash", comment = (string?)null });
        await Add("/api/expenses", new { date = "2047-03-20", payer = "Vitaliy", categoryId = pets, amount = 200m, paymentMethod = "Cash", comment = (string?)null });
        await Add("/api/expenses/incomes", new { date = "2047-03-05", person = "Maria", amount = 4_000m, comment = (string?)null });
        await Add("/api/expenses/savings", new { date = "2047-03-06", person = "Vitaliy", amount = 1_000m, comment = (string?)null });

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/expenses/summary?year=2047&month=3");

        Assert.Equal(4_000m, summary.GetProperty("incomes").GetDecimal());
        Assert.Equal(1_700m, summary.GetProperty("expenses").GetDecimal());
        Assert.Equal(1_000m, summary.GetProperty("savingsNet").GetDecimal());
        Assert.Equal(4_000m - 1_700m - 1_000m, summary.GetProperty("result").GetDecimal());
        Assert.Equal(JsonValueKind.Null, summary.GetProperty("daysLeft").ValueKind);
        var limits = summary.GetProperty("limits").EnumerateArray().ToDictionary(l => l.GetProperty("name").GetString()!);
        Assert.Equal(1_500m, limits["Еда и быт"].GetProperty("spent").GetDecimal());
        Assert.Equal(10, limits["Еда и быт"].GetProperty("percent").GetInt32());
        // Quarterly: February and March of the first quarter both count.
        Assert.Equal(500m, limits["Зоотовары"].GetProperty("spent").GetDecimal());
        Assert.Equal("2047-01-01", limits["Зоотовары"].GetProperty("from").GetString());
        Assert.Equal("2047-03-31", limits["Зоотовары"].GetProperty("to").GetString());
        var top = summary.GetProperty("topCategories");
        Assert.Equal(groceries, top[0].GetProperty("categoryId").GetGuid());
        Assert.Equal(3, top.GetArrayLength());
    }

    private static JsonElement Find(JsonElement limits, Guid id) =>
        limits.EnumerateArray().Single(l => l.GetProperty("id").GetGuid() == id);

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

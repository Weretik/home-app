using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Expenses.Budget;

/// <summary>
/// Overspend carrying over like the old «Бюджет» sheet, and expenses marked «Не в лимиты» counting in the totals
/// only. Each test uses its own far-future year, and every overspend is paid off within that year, so the other
/// tests' months see no carry.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesCarryOverApiTests(ApiFactory api)
{
    private static readonly Guid Groceries = new("86c6ddca-a991-5d9b-97b9-468b90f50c4c");
    private static readonly Guid Repairs = new("eec40aeb-c2a3-5aea-93e8-d4f4929c79aa");
    private static readonly Guid Pets = new("08305dd6-28c2-59a1-9d98-0026828b4eee");

    [SkippableFact]
    public async Task Overspend_CarriesOver_FromLimitsFrom_AndOutsideLimits_NeverCounts()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        async Task Add(string date, Guid category, decimal amount, bool outsideLimits = false) =>
            Assert.Equal(HttpStatusCode.Created, (await PostAsync(client, "/api/expenses", new
            {
                date, payer = "Shared", categoryId = category, amount, paymentMethod = "Card", comment = (string?)null, outsideLimits,
            })).StatusCode);

        // January is before LimitsFrom for the monthly limits, but the quarter holding LimitsFrom starts in January.
        await Add("2048-01-10", Groceries, 20_000m);
        await Add("2048-01-11", Pets, 3_000m);
        await Add("2048-02-10", Groceries, 18_000m);
        await Add("2048-02-11", Repairs, 40_000m);
        await Add("2048-02-12", Groceries, 100_000m, outsideLimits: true);
        await Add("2048-03-10", Groceries, 14_000m);
        await Add("2048-03-11", Groceries, 7_000m, outsideLimits: true);

        await SetLimitsFromAsync(client, "2048-02-01");
        try
        {
            var january = await SummaryAsync(client, 2048, 1);
            var food = Limit(january, "Еда и быт");
            Assert.Equal((20_000m, 0m, -5_000m, 0m), (Spent(food), CarriedIn(food), Left(food), CarriesOut(food)));
            Assert.Equal(0m, january.GetProperty("carriedIn").GetDecimal());
            Assert.Equal(0m, january.GetProperty("carriesOut").GetDecimal());

            var february = await SummaryAsync(client, 2048, 2);
            food = Limit(february, "Еда и быт");
            Assert.Equal((18_000m, 0m, -3_000m, 3_000m), (Spent(food), CarriedIn(food), Left(food), CarriesOut(food)));
            Assert.Equal(158_000m, february.GetProperty("expenses").GetDecimal());
            Assert.Equal(1, february.GetProperty("outsideLimitsCount").GetInt32());
            Assert.Equal(100_000m, february.GetProperty("outsideLimitsAmount").GetDecimal());
            Assert.Equal(8_000m, february.GetProperty("carriesOut").GetDecimal());

            var march = await SummaryAsync(client, 2048, 3);
            Assert.Equal(21_000m, march.GetProperty("expenses").GetDecimal());
            Assert.Equal(1, march.GetProperty("outsideLimitsCount").GetInt32());
            Assert.Equal(7_000m, march.GetProperty("outsideLimitsAmount").GetDecimal());
            food = Limit(march, "Еда и быт");
            Assert.Equal((14_000m, 3_000m, -2_000m, 2_000m), (Spent(food), CarriedIn(food), Left(food), CarriesOut(food)));
            Assert.Equal(113, food.GetProperty("percent").GetInt32());
            var other = Limit(march, "Другое");
            Assert.Equal((0m, 25_000m, -10_000m, 10_000m), (Spent(other), CarriedIn(other), Left(other), CarriesOut(other)));
            var pets = Limit(march, "Зоотовары");
            Assert.Equal((3_000m, 0m, -500m, 500m), (Spent(pets), CarriedIn(pets), Left(pets), CarriesOut(pets)));
            // The monthly total: February counted 58 000 against 50 000; the 100 000 outside the limits does not count.
            Assert.Equal(8_000m, march.GetProperty("carriedIn").GetDecimal());
            Assert.Equal(0m, march.GetProperty("carriesOut").GetDecimal());
            Assert.Equal(50_000m - 14_000m - 8_000m, march.GetProperty("forecast").GetProperty("expectedTotalLeft").GetDecimal());

            var april = await SummaryAsync(client, 2048, 4);
            Assert.Equal(2_000m, CarriedIn(Limit(april, "Еда и быт")));
            Assert.Equal(10_000m, CarriedIn(Limit(april, "Другое")));
            pets = Limit(april, "Зоотовары");
            Assert.Equal(("2048-04-01", 500m, 2_000m), (pets.GetProperty("from").GetString(), CarriedIn(pets), Left(pets)));
            Assert.Equal(0m, april.GetProperty("carriedIn").GetDecimal());

            var may = await SummaryAsync(client, 2048, 5);
            Assert.Equal(0m, CarriedIn(Limit(may, "Еда и быт")));
            Assert.Equal(0m, CarriedIn(Limit(may, "Другое")));
        }
        finally
        {
            await SetLimitsFromAsync(client, "2026-11-01");
        }
    }

    [SkippableFact]
    public async Task An_Expense_MarkedOutsideLimits_CountsInTheTotals_ButInNoLimit()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var body = new { date = "2049-06-05", payer = "Maria", categoryId = Groceries, amount = 500m, paymentMethod = "Card", comment = (string?)null };

        var created = await PostAsync(client, "/api/expenses", body);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var item = (await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2049&month=6")).GetProperty("items")[0];
        Assert.False(item.GetProperty("outsideLimits").GetBoolean());

        var before = await SummaryAsync(client, 2049, 6);
        Assert.Equal(500m, Spent(Limit(before, "Еда и быт")));
        Assert.Equal(0, before.GetProperty("outsideLimitsCount").GetInt32());

        var marked = await PutAsync(client, $"/api/expenses/{id}", item.GetProperty("version").GetUInt32(), new
        {
            body.date, body.payer, body.categoryId, body.amount, body.paymentMethod, body.comment, outsideLimits = true,
        });
        Assert.Equal(HttpStatusCode.NoContent, marked.StatusCode);

        var list = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2049&month=6");
        Assert.True(list.GetProperty("items")[0].GetProperty("outsideLimits").GetBoolean());
        Assert.Equal(500m, list.GetProperty("totalAmount").GetDecimal());

        var after = await SummaryAsync(client, 2049, 6);
        Assert.Equal(500m, after.GetProperty("expenses").GetDecimal());
        Assert.Equal(500m, after.GetProperty("expensesByPayer")[0].GetProperty("amount").GetDecimal());
        Assert.Equal(Groceries, after.GetProperty("topCategories")[0].GetProperty("categoryId").GetGuid());
        Assert.Equal(-500m, after.GetProperty("result").GetDecimal());
        Assert.Equal(1, after.GetProperty("outsideLimitsCount").GetInt32());
        Assert.Equal(500m, after.GetProperty("outsideLimitsAmount").GetDecimal());
        var food = Limit(after, "Еда и быт");
        Assert.Equal((0m, 15_000m, 0), (Spent(food), Left(food), food.GetProperty("percent").GetInt32()));
        Assert.Equal(50_000m, after.GetProperty("forecast").GetProperty("expectedTotalLeft").GetDecimal());
    }

    private static async Task SetLimitsFromAsync(HttpClient client, string limitsFrom)
    {
        var budget = await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget");
        var changed = await PutAsync(client, "/api/expenses/budget/settings", budget.GetProperty("version").GetUInt32(), new
        {
            monthlyLimit = budget.GetProperty("monthlyLimit").GetDecimal(),
            savingsGoal = budget.GetProperty("savingsGoal").GetDecimal(),
            limitsFrom,
        });
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        Assert.Equal(limitsFrom, (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limitsFrom").GetString());
    }

    private static Task<JsonElement> SummaryAsync(HttpClient client, int year, int month) =>
        client.GetFromJsonAsync<JsonElement>($"/api/expenses/summary?year={year}&month={month}");

    private static JsonElement Limit(JsonElement summary, string name) =>
        summary.GetProperty("limits").EnumerateArray().Single(l => l.GetProperty("name").GetString() == name);

    private static decimal Spent(JsonElement limit) => limit.GetProperty("spent").GetDecimal();

    private static decimal CarriedIn(JsonElement limit) => limit.GetProperty("carriedIn").GetDecimal();

    private static decimal Left(JsonElement limit) => limit.GetProperty("left").GetDecimal();

    private static decimal CarriesOut(JsonElement limit) => limit.GetProperty("carriesOut").GetDecimal();

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PutAsync(HttpClient client, string url, uint version, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }
}

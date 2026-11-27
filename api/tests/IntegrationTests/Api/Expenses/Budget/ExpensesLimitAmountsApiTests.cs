using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.Api.Notifications;
using IntegrationTests.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Api.Expenses.Budget;

/// <summary>
/// Limit amounts kept by the month they start in (SDD 006). The first test's host clock stands on 10 December 2001, a
/// year no other test uses and earlier than any session's expiry; the limits start counting from November 2001 during
/// the test, and the start day and the monthly limit (one step of 50 000) are put back at the end.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesLimitAmountsApiTests(ApiFactory api)
{
    private static readonly Guid Groceries = new("86c6ddca-a991-5d9b-97b9-468b90f50c4c");

    [SkippableFact]
    public async Task A_Lowered_MonthlyLimit_AppliesFromTheCurrentMonth_AndNovember_KeepsItsOwn()
    {
        // SC-LA-001, SC-LA-008, SC-LA-009
        Skip.If(api.Unavailable is not null, api.Unavailable);
        await using var host = api.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddSingleton<TimeProvider>(new FixedClock(new DateTimeOffset(2001, 12, 10, 8, 0, 0, TimeSpan.Zero)))));
        using var client = api.SignedIn(host.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") }));

        var seeded = await BudgetAsync(client);
        Assert.Equal([("2000-01-01", 50_000m)], Steps(seeded.GetProperty("monthlyLimits")));
        Assert.All(seeded.GetProperty("limits").EnumerateArray(), l =>
            Assert.Equal([("2000-01-01", l.GetProperty("amount").GetDecimal())], Steps(l.GetProperty("amounts"))));

        var limitsFrom = seeded.GetProperty("limitsFrom").GetString()!;
        try
        {
            Assert.Equal(HttpStatusCode.NoContent, (await SettingsAsync(client, 7_000m, "2001-11-01", "2001-11-01")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await SettingsAsync(client, 5_000m, null)).StatusCode);

            var budget = await BudgetAsync(client);
            Assert.Equal(5_000m, budget.GetProperty("monthlyLimit").GetDecimal());
            Assert.Equal(
                [("2000-01-01", 50_000m), ("2001-11-01", 7_000m), ("2001-12-01", 5_000m)],
                Steps(budget.GetProperty("monthlyLimits")));

            Assert.Equal(HttpStatusCode.Created, (await PostAsync(client, "/api/expenses", Expense("2001-11-05", 8_000m))).StatusCode);
            Assert.Equal(HttpStatusCode.Created, (await PostAsync(client, "/api/expenses", Expense("2001-12-05", 4_000m))).StatusCode);

            var november = await client.GetFromJsonAsync<JsonElement>("/api/expenses/summary?year=2001&month=11");
            var december = await client.GetFromJsonAsync<JsonElement>("/api/expenses/summary?year=2001&month=12");
            Assert.Equal((7_000m, 0m, 1_000m), Total(november));
            Assert.Equal((5_000m, 1_000m, 0m), Total(december));
            Assert.Equal(5_000m - 4_000m - 1_000m, december.GetProperty("forecast").GetProperty("expectedTotalLeft").GetDecimal());

            var tooEarly = await SettingsAsync(client, 1m, "1999-12-01");
            Assert.Equal(HttpStatusCode.BadRequest, tooEarly.StatusCode);
            Assert.Equal("application/problem+json", tooEarly.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            Assert.Equal(HttpStatusCode.NoContent, (await SettingsAsync(client, 50_000m, "2001-11-01", limitsFrom)).StatusCode);
            Assert.Equal([("2000-01-01", 50_000m)], Steps((await BudgetAsync(client)).GetProperty("monthlyLimits")));
        }
    }

    [SkippableFact]
    public async Task A_Limit_Changed_FromAMonth_KeepsTheOldAmount_ForEarlierMonths()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var name = $"Шаги {Guid.NewGuid():N}"[..20];
        var created = await PostAsync(client, "/api/expenses/budget/limits",
            new { name, period = "Month", amount = 7_000m, categoryIds = new[] { Groceries }, amountFrom = "2057-03-01" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        try
        {
            var limit = Find(await BudgetAsync(client), id);
            // A new limit ignores amountFrom: its amount applies to every period.
            Assert.Equal([("2000-01-01", 7_000m)], Steps(limit.GetProperty("amounts")));

            var changed = await PutAsync(client, $"/api/expenses/budget/limits/{id}", limit.GetProperty("version").GetUInt32(),
                new { name, period = "Month", amount = 5_000m, categoryIds = new[] { Groceries }, amountFrom = "2057-02-14" });
            Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

            limit = Find(await BudgetAsync(client), id);
            Assert.Equal([("2000-01-01", 7_000m), ("2057-02-01", 5_000m)], Steps(limit.GetProperty("amounts")));
            Assert.Equal(7_000m, limit.GetProperty("amount").GetDecimal());
            Assert.Equal(7_000m, UsageAmount(await client.GetFromJsonAsync<JsonElement>("/api/expenses/summary?year=2057&month=1"), id));
            Assert.Equal(5_000m, UsageAmount(await client.GetFromJsonAsync<JsonElement>("/api/expenses/summary?year=2057&month=2"), id));

            var outOfRange = await PutAsync(client, $"/api/expenses/budget/limits/{id}", limit.GetProperty("version").GetUInt32(),
                new { name, period = "Month", amount = 1m, categoryIds = new[] { Groceries }, amountFrom = "1999-12-01" });
            Assert.Equal(HttpStatusCode.BadRequest, outOfRange.StatusCode);
        }
        finally
        {
            await client.DeleteAsync($"/api/expenses/budget/limits/{id}");
        }
    }

    private static object Expense(string date, decimal amount) =>
        new { date, payer = "Shared", categoryId = Groceries, amount, paymentMethod = "Card", comment = (string?)null };

    private static (decimal Limit, decimal CarriedIn, decimal CarriesOut) Total(JsonElement summary) => (
        summary.GetProperty("monthlyLimit").GetDecimal(),
        summary.GetProperty("carriedIn").GetDecimal(),
        summary.GetProperty("carriesOut").GetDecimal());

    private static decimal UsageAmount(JsonElement summary, Guid id) =>
        summary.GetProperty("limits").EnumerateArray().Single(l => l.GetProperty("id").GetGuid() == id).GetProperty("amount").GetDecimal();

    private static List<(string, decimal)> Steps(JsonElement steps) =>
        steps.EnumerateArray().Select(s => (s.GetProperty("from").GetString()!, s.GetProperty("amount").GetDecimal())).ToList();

    private static JsonElement Find(JsonElement budget, Guid id) =>
        budget.GetProperty("limits").EnumerateArray().Single(l => l.GetProperty("id").GetGuid() == id);

    private static Task<JsonElement> BudgetAsync(HttpClient client) => client.GetFromJsonAsync<JsonElement>("/api/expenses/budget");

    private static async Task<HttpResponseMessage> SettingsAsync(
        HttpClient client, decimal monthlyLimit, string? monthlyLimitFrom, string? limitsFrom = null)
    {
        var budget = await BudgetAsync(client);
        return await PutAsync(client, "/api/expenses/budget/settings", budget.GetProperty("version").GetUInt32(), new
        {
            monthlyLimit,
            savingsGoal = budget.GetProperty("savingsGoal").GetDecimal(),
            limitsFrom,
            monthlyLimitFrom,
        });
    }

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

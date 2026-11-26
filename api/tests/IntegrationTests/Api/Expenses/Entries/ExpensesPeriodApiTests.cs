using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Expenses.Entries;

/// <summary>
/// The expense list over a period of days (from, to, both included) instead of a month. The test uses its own
/// years, so tests sharing the database do not see each other's rows.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesPeriodApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task A_Period_ListsItsDays_BothEndsIncluded_WithTheirTotal()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var category = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/categories"))[0].GetProperty("id").GetGuid();
        foreach (var (date, amount) in new[]
                 {
                     ("2044-12-31", 1m), ("2045-01-01", 10m), ("2045-02-15", 20.50m), ("2045-03-31", 30m), ("2045-04-01", 1_000m),
                 })
        {
            var created = await PostAsync(client, "/api/expenses", new
            {
                date, payer = "Shared", categoryId = category, amount, paymentMethod = "Card", comment = (string?)null,
            });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        }

        var quarter = await client.GetFromJsonAsync<JsonElement>("/api/expenses?from=2045-01-01&to=2045-03-31");
        Assert.Equal(3, quarter.GetProperty("totalCount").GetInt32());
        Assert.Equal(60.50m, quarter.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(
            ["2045-03-31", "2045-02-15", "2045-01-01"],
            quarter.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("date").GetString()));

        var oneDay = await client.GetFromJsonAsync<JsonElement>("/api/expenses?from=2045-04-01&to=2045-04-01");
        Assert.Equal(1_000m, oneDay.GetProperty("totalAmount").GetDecimal());

        var acrossYears = await client.GetFromJsonAsync<JsonElement>("/api/expenses?from=2044-12-31&to=2045-01-01&pageSize=1");
        Assert.Equal(2, acrossYears.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, acrossYears.GetProperty("items").GetArrayLength());
        Assert.Equal(11m, acrossYears.GetProperty("totalAmount").GetDecimal());

        var wholeYear = await client.GetFromJsonAsync<JsonElement>("/api/expenses?from=2044-04-02&to=2045-04-01");
        Assert.Equal(5, wholeYear.GetProperty("totalCount").GetInt32());

        var february = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2045&month=2");
        Assert.Equal(20.50m, february.GetProperty("totalAmount").GetDecimal());
    }

    [SkippableTheory]
    [InlineData("from=2045-03-31&to=2045-01-01")]
    [InlineData("from=2045-01-01&to=2046-01-02")]
    [InlineData("year=2045&month=1&from=2045-01-01&to=2045-01-31")]
    [InlineData("from=2045-01-01")]
    [InlineData("year=2045")]
    [InlineData("")]
    public async Task A_Wrong_Period_IsAValidationProblem(string queryString)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await client.GetAsync($"/api/expenses?{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }
}

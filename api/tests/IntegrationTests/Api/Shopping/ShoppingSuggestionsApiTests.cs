using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Shopping.ShoppingRequests;

namespace IntegrationTests.Api.Shopping;

/// <summary>
/// Name suggestions for the quick add (SDD shopping 002): matching, ranking, the latest spelling, «in the list» and
/// input checks. Every name starts with a unique suffix, so a query by the suffix sees only this test's rows.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ShoppingSuggestionsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task NamesStartingWithTheQuery_ComeFirst_ThenTheMostBought()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        await BuyAsync(client, $"{u} молоко");
        await BuyAsync(client, $"{u} молоко");
        await BuyAsync(client, $"йогурт {u}");
        await AddAsync(client, $"{u} мука");

        var all = await SuggestAsync(client, u);
        var milk = await SuggestAsync(client, $"{u} мол");

        Assert.Equal([$"{Cap(u)} молоко", $"{Cap(u)} мука", $"Йогурт {u}"], all.Select(s => s.Text("name")));
        Assert.Equal([2, 0, 1], all.Select(s => s.GetProperty("timesBought").GetInt32()));
        Assert.Equal([$"{Cap(u)} молоко"], milk.Select(s => s.Text("name")));
    }

    [SkippableFact]
    public async Task OneSuggestionPerName_InItsLatestSpelling_AndDepartment()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var tea = await DepartmentAsync(client, "Чай, кофе, напитки");
        var other = await DepartmentAsync(client, "Другое");
        await BuyAsync(client, $"{u} зелёный чай", other.Id());
        await AddAsync(client, $"{u} зеленый чай", departmentId: tea.Id());

        var found = Assert.Single(await SuggestAsync(client, $"{u.ToUpperInvariant()} ЗЕЛЁ"));

        Assert.Equal($"{Cap(u)} зеленый чай", found.Text("name"));
        Assert.Equal(tea.Id(), found.GetProperty("departmentId").GetGuid());
        Assert.Equal("Чай, кофе, напитки", found.Text("departmentName"));
        Assert.Equal(1, found.GetProperty("timesBought").GetInt32());
        Assert.True(found.GetProperty("inList").GetBoolean());
        Assert.True(found.GetProperty("lastAddedAtUtc").GetDateTimeOffset() > DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    [SkippableFact]
    public async Task InList_MarksNeededAndCartNames_NotHistoryOnes()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        await BuyAsync(client, $"{u} хлебцы");
        await AddAsync(client, $"{u} хлеб");
        var basket = (await AddAsync(client, $"{u} хлебница")).GetProperty("added")[0];
        await PutAsync(client, $"/items/{basket.Id()}/status", new { status = "Bought", by = "Maria" });

        var found = (await SuggestAsync(client, $"{u} хле")).ToDictionary(s => s.Text("name"), s => s.GetProperty("inList").GetBoolean());

        Assert.False(found[$"{Cap(u)} хлебцы"]);
        Assert.True(found[$"{Cap(u)} хлеб"]);
        Assert.True(found[$"{Cap(u)} хлебница"]);
    }

    [SkippableFact]
    public async Task Wildcards_InTheQuery_AreLiteral()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        await AddAsync(client, $"{u}_a; {u}ba; {u}%c");

        var underscore = await SuggestAsync(client, $"{u}_");
        var percent = await SuggestAsync(client, $"{u}%");

        Assert.Equal([$"{Cap(u)}_a"], underscore.Select(s => s.Text("name")));
        Assert.Equal([$"{Cap(u)}%c"], percent.Select(s => s.Text("name")));
    }

    [SkippableFact]
    public async Task The_Limit_DefaultsToEight()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        await AddAsync(client, string.Join(", ", "abcdefghijkl".Select(c => $"{u} n{c}")));

        Assert.Equal(8, (await SuggestAsync(client, u)).Count);
        Assert.Equal(12, (await SuggestAsync(client, u, limit: 20)).Count);
        Assert.Empty(await SuggestAsync(client, $"{u} nz"));
    }

    [SkippableTheory]
    [InlineData("", "query")]
    [InlineData("?query=%20%20", "query")]
    [InlineData("?query=молоко&limit=0", "limit")]
    [InlineData("?query=молоко&limit=21", "limit")]
    public async Task WrongInput_IsAValidationProblem(string queryString, string field)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await client.GetAsync($"{Root}/items/suggestions{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains(
            problem.GetProperty("errors").EnumerateObject(),
            e => string.Equals(e.Name, field, StringComparison.OrdinalIgnoreCase));
    }

    [SkippableFact]
    public async Task A_QueryOver80Characters_IsAValidationProblem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await client.GetAsync($"{Root}/items/suggestions?query={new string('a', 81)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<List<JsonElement>> SuggestAsync(HttpClient client, string query, int? limit = null)
    {
        var url = $"/items/suggestions?query={Uri.EscapeDataString(query)}" + (limit is { } l ? $"&limit={l}" : string.Empty);
        return (await GetAsync(client, url)).EnumerateArray().ToList();
    }

    /// <summary>Adds the name, ticks it bought and clears the cart, so it is one history row.</summary>
    private static async Task BuyAsync(HttpClient client, string name, Guid? departmentId = null)
    {
        var item = (await AddAsync(client, name, departmentId: departmentId)).GetProperty("added")[0];
        await PutAsync(client, $"/items/{item.Id()}/status", new { status = "Bought", by = "Vitaliy" });
        await client.PostAsync($"{Root}/items/clear-cart", null);
    }

    /// <summary>The parser starts a name with a capital letter.</summary>
    private static string Cap(string text) => char.ToUpperInvariant(text[0]) + text[1..];
}

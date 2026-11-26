using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Shopping.ShoppingRequests;

namespace IntegrationTests.Api.Shopping;

/// <summary>The list end to end: add from text, re-add, tick, cart, history, frequent items and the summary.</summary>
[Collection(ApiCollection.Name)]
public sealed class ShoppingItemsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Text_IsSplitIntoItems_InTheirDepartments()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();

        var result = await AddAsync(client, $"молоко {u}a, хлеб {u}b, яйца {u}c 10, сыр {u}d 300 г, 2 кг картошки {u}e", "Maria");

        var added = result.GetProperty("added").EnumerateArray().ToList();
        Assert.Equal(5, added.Count);
        Assert.Empty(result.GetProperty("restored").EnumerateArray());
        Assert.Equal(
            ["Молочное и яйца", "Хлеб", "Молочное и яйца", "Сыр и колбаса", "Овощи и фрукты"],
            added.Select(i => i.Text("departmentName")));
        Assert.Equal("10", added[2].Text("quantity"));
        Assert.Equal("300 г", added[3].Text("quantity"));
        Assert.Equal("2 кг", added[4].Text("quantity"));
        Assert.Equal($"Картошки {u}e", added[4].Text("name"));
        Assert.All(added, i => Assert.Equal("Maria", i.Text("addedBy")));
        Assert.All(added, i => Assert.Equal("Needed", i.Text("status")));

        var milk = await InListAsync(client, $"Молоко {u}a");
        Assert.NotNull(milk);
        Assert.Equal("🥛", milk.Value.Text("departmentIcon"));
    }

    [SkippableFact]
    public async Task ReAdding_UpdatesANeededItem_AndRestoresOneFromTheCart()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var milk = (await AddAsync(client, $"молоко {u} 1")).GetProperty("added")[0];
        var bread = (await AddAsync(client, $"хлеб {u}")).GetProperty("added")[0];
        await PutAsync(client, $"/items/{bread.Id()}/status", new { status = "Bought", by = "Vitaliy" });

        var again = await AddAsync(client, $"молоко {u} 2, хлеб {u}", "Maria");

        var updated = Assert.Single(again.GetProperty("updated").EnumerateArray());
        Assert.Equal(milk.Id(), updated.Id());
        Assert.Equal("2", updated.Text("quantity"));
        var restored = Assert.Single(again.GetProperty("restored").EnumerateArray());
        Assert.Equal(bread.Id(), restored.Id());
        Assert.Equal("Needed", restored.Text("status"));
        Assert.Equal("Maria", restored.Text("addedBy"));
        Assert.Equal(JsonValueKind.Null, restored.GetProperty("boughtBy").ValueKind);
        Assert.Empty(again.GetProperty("added").EnumerateArray());
    }

    [SkippableFact]
    public async Task Ticking_IsATargetValue_AndAnEdit_NeedsTheVersion()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var item = (await AddAsync(client, $"кефир {u}")).GetProperty("added")[0];

        var first = await (await PutAsync(client, $"/items/{item.Id()}/status", new { status = "Bought", by = "Vitaliy" }))
            .Content.ReadFromJsonAsync<JsonElement>();
        var second = await (await PutAsync(client, $"/items/{item.Id()}/status", new { status = "Bought", by = "Maria" }))
            .Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Vitaliy", second.Text("boughtBy"));
        Assert.Equal(first.Text("boughtAtUtc"), second.Text("boughtAtUtc"));

        var needed = await (await PutAsync(client, $"/items/{item.Id()}/status", new { status = "Needed", by = "Maria" }))
            .Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Needed", needed.Text("status"));
        Assert.Equal(JsonValueKind.Null, needed.GetProperty("boughtBy").ValueKind);

        var body = new { name = $"Кефир {u} 2,5%", quantity = "2", note = "без лактозы", departmentId = item.GetProperty("departmentId").GetGuid(), urgent = true };
        Assert.Equal(HttpStatusCode.BadRequest, (await PutAsync(client, $"/items/{item.Id()}", body)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await PutAsync(client, $"/items/{item.Id()}", body, item.Version())).StatusCode);
        var edited = await PutAsync(client, $"/items/{item.Id()}", body, needed.Version());
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        var view = await edited.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("без лактозы", view.Text("note"));
        Assert.True(view.GetProperty("urgent").GetBoolean());

        Assert.Equal(HttpStatusCode.Conflict, (await DeleteAsync(client, $"/items/{item.Id()}", needed.Version())).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await DeleteAsync(client, $"/items/{item.Id()}", view.Version())).StatusCode);
        Assert.Null(await InListAsync(client, view.Text("name")));
    }

    [SkippableFact]
    public async Task Clearing_TheCart_MovesItemsToHistory_WhereTheyNoLongerChange()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var added = (await AddAsync(client, $"батон {u}, сок {u}")).GetProperty("added").EnumerateArray().ToList();
        foreach (var item in added)
            await PutAsync(client, $"/items/{item.Id()}/status", new { status = "Bought", by = "Maria" });

        var clear = await client.PostAsync($"{Root}/items/clear-cart", null);
        var again = await client.PostAsync($"{Root}/items/clear-cart", null);

        Assert.True((await clear.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("cleared").GetInt32() >= 2);
        Assert.Equal(0, (await again.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("cleared").GetInt32());
        Assert.Null(await InListAsync(client, $"Батон {u}"));

        var history = await GetAsync(client, "/history?page=1&pageSize=10");
        var rows = history.GetProperty("items").EnumerateArray().ToList();
        Assert.Contains(rows, r => r.Text("name") == $"Батон {u}" && r.Text("boughtBy") == "Maria");
        Assert.True(history.GetProperty("totalCount").GetInt32() >= 2);

        var bread = rows.First(r => r.Text("name") == $"Батон {u}");
        var toggle = await PutAsync(client, $"/items/{bread.Id()}/status", new { status = "Needed", by = "Maria" });
        Assert.Equal(HttpStatusCode.NotFound, toggle.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await DeleteAsync(client, $"/items/{bread.Id()}", bread.Version())).StatusCode);

        // «Вернуть в список» is an ordinary add: a new row, the history row stays.
        var back = await AddAsync(client, $"сок {u}");
        Assert.Single(back.GetProperty("added").EnumerateArray());
    }

    [SkippableFact]
    public async Task History_AndFrequent_CountPurchases_ByName()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var name = $"Ряженка {Unique()}";
        for (var i = 0; i < 3; i++)
        {
            var item = (await AddAsync(client, name)).GetProperty("added")[0];
            await PutAsync(client, $"/items/{item.Id()}/status", new { status = "Bought", by = "Vitaliy" });
            await client.PostAsync($"{Root}/items/clear-cart", null);
        }

        var frequent = await GetAsync(client, "/frequent");
        var entry = frequent.EnumerateArray().Single(f => f.Text("name") == name);
        Assert.Equal(3, entry.GetProperty("timesBought").GetInt32());
        Assert.Equal("Молочное и яйца", entry.Text("departmentName"));

        await AddAsync(client, name);
        var whileNeeded = await GetAsync(client, "/frequent");
        Assert.DoesNotContain(whileNeeded.EnumerateArray(), f => f.Text("name") == name);
    }

    [SkippableFact]
    public async Task A_Repeated_Add_ReturnsTheSameItems_AndOtherData_IsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var key = Guid.NewGuid().ToString();
        var body = new { text = $"чай {u}, кофе {u}", addedBy = "Maria", departmentId = (Guid?)null, urgent = false };

        var first = await PostAsync(client, "/items", body, key);
        var repeat = await PostAsync(client, "/items", body, key);
        var other = await PostAsync(client, "/items", body with { text = $"какао {u}" }, key);
        var noKey = await client.PostAsJsonAsync($"{Root}/items", body);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
        Assert.Equal(
            (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("added").EnumerateArray().Select(i => i.Id()),
            (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("added").EnumerateArray().Select(i => i.Id()));
        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noKey.StatusCode);
        var list = await GetAsync(client, "/items");
        Assert.Single(list.GetProperty("items").EnumerateArray(), i => i.Text("name") == $"Чай {u}");
    }

    [SkippableFact]
    public async Task The_Summary_CountsTheList()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        await AddAsync(client, $"йогурт {u}", urgent: true);

        var summary = await GetAsync(client, "/summary");

        Assert.True(summary.GetProperty("neededCount").GetInt32() >= 1);
        Assert.True(summary.GetProperty("urgentCount").GetInt32() >= 1);
        var next = summary.GetProperty("nextItems").EnumerateArray().ToList();
        Assert.InRange(next.Count, 1, 8);
        Assert.True(next[0].GetProperty("urgent").GetBoolean());
        Assert.Contains(summary.GetProperty("departments").EnumerateArray(), d => d.Text("name") == "Молочное и яйца");
        var orders = summary.GetProperty("departments").EnumerateArray().Select(d => d.Text("name")).ToList();
        Assert.Equal(orders.Distinct().Count(), orders.Count);
    }

    [SkippableFact]
    public async Task An_Add_WithoutItems_IsAValidationProblem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await PostAsync(client, "/items", new { text = " , ; ", addedBy = "Maria" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

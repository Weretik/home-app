using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Gifts.GiftsApi;

namespace IntegrationTests.Api.Gifts;

/// <summary>
/// Gifts end to end: add with variants, repeat, edit, stale edit, status moves and delete. Tests share the database, so
/// they look only at the gifts they created.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class GiftsApiTests(ApiFactory api)
{
    private const string Jewellery = "b5691e11-6405-5690-bba9-43e92fef7366";
    private const string Tech = "a1b9482e-bc3b-53cd-8232-0dbe2b4aa249";

    private static object Gift(string name, string priority = "High", string? occasionDate = null, object[]? variants = null) => new
    {
        recipient = "Maria",
        name,
        categoryId = Jewellery,
        priority,
        price = (decimal?)null,
        link = (string?)null,
        occasion = "День рождения",
        occasionDate,
        notes = (string?)null,
        variants = variants ?? [],
    };

    [SkippableFact]
    public async Task A_Gift_WithVariants_IsAdded_Edited_AndDeleted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var created = await PostAsync(client, "/api/gifts/items", Gift("  Кольцо с камнем ", occasionDate: "2040-03-14", variants:
        [
            new { title = "Pandora", link = "https://pandora.example/ring", price = 4_500.50m },
            new { title = (string?)null, link = "https://shop.example/r", price = (decimal?)null },
        ]));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = await IdOfAsync(created);

        var item = await ItemAsync(client, id);
        Assert.Equal("Кольцо с камнем", item.GetProperty("name").GetString());
        Assert.Equal("Wanted", item.GetProperty("status").GetString());
        Assert.Equal("Украшения", item.GetProperty("categoryName").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("price").ValueKind);
        Assert.Equal("2040-03-14", item.GetProperty("occasionDate").GetString());
        var variants = item.GetProperty("variants");
        Assert.Equal(2, variants.GetArrayLength());
        Assert.Equal("Pandora", variants[0].GetProperty("title").GetString());
        Assert.Equal(4_500.50m, variants[0].GetProperty("price").GetDecimal());
        var version = item.GetProperty("version").GetUInt32();

        var edited = await PutAsync(client, $"/api/gifts/items/{id}", version, new
        {
            recipient = "Together",
            name = "Плед",
            categoryId = Tech,
            priority = "Someday",
            price = 900m,
            link = "https://home.example/pled",
            occasion = (string?)null,
            occasionDate = (string?)null,
            notes = "мягкий",
            variants = new[] { new { title = "IKEA", link = "https://ikea.example/p", price = (decimal?)950m } },
        });
        Assert.Equal(HttpStatusCode.NoContent, edited.StatusCode);
        item = await ItemAsync(client, id);
        Assert.Equal("Плед", item.GetProperty("name").GetString());
        Assert.Equal("Together", item.GetProperty("recipient").GetString());
        Assert.Equal(900m, item.GetProperty("price").GetDecimal());
        Assert.Equal("IKEA", Assert.Single(item.GetProperty("variants").EnumerateArray()).GetProperty("title").GetString());

        var stale = await PutAsync(client, $"/api/gifts/items/{id}", version, Gift("Старое"));
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("gifts.changed_meanwhile", await CodeOfAsync(stale));
        Assert.Equal("Плед", (await ItemAsync(client, id)).GetProperty("name").GetString());

        var noVersion = await client.PutAsJsonAsync($"/api/gifts/items/{id}", Gift("Без версии"));
        Assert.Equal(HttpStatusCode.BadRequest, noVersion.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/gifts/items/{id}")).StatusCode);
        var again = await client.DeleteAsync($"/api/gifts/items/{id}");
        Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
        Assert.Equal("gifts.gift_not_found", await CodeOfAsync(again));
    }

    [SkippableFact]
    public async Task Changing_Only_The_Variants_MovesTheVersion()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Серьги")));
        var version = (await ItemAsync(client, id)).GetProperty("version").GetUInt32();

        var edited = await PutAsync(client, $"/api/gifts/items/{id}", version,
            Gift("Серьги", variants: [new { title = "A", link = "https://a.example", price = (decimal?)null }]));
        Assert.Equal(HttpStatusCode.NoContent, edited.StatusCode);

        var after = await ItemAsync(client, id);
        Assert.NotEqual(version, after.GetProperty("version").GetUInt32());
        var stale = await PutAsync(client, $"/api/gifts/items/{id}", version, Gift("Серьги"));
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
    }

    [SkippableFact]
    public async Task A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var key = Guid.NewGuid().ToString();

        var first = await PostAsync(client, "/api/gifts/items", Gift("Браслет"), key);
        var repeat = await PostAsync(client, "/api/gifts/items", Gift("Браслет"), key);
        var other = await PostAsync(client, "/api/gifts/items", Gift("Другой браслет"), key);

        Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
        Assert.Equal(await IdOfAsync(first), await IdOfAsync(repeat));
        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        Assert.Equal("gifts.idempotency_key_reused", await CodeOfAsync(other));
    }

    [SkippableFact]
    public async Task Invalid_Input_IsAValidationProblem_AndAnUnknownCategory_IsNotFound()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var badLink = await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Vitaliy", name = "Часы", categoryId = Tech, priority = "Normal", link = "ftp://x.example", variants = Array.Empty<object>(),
        });
        var badPrice = await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Vitaliy", name = "Часы", categoryId = Tech, priority = "Normal", price = 1.234m, variants = Array.Empty<object>(),
        });
        var unknownCategory = await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Vitaliy", name = "Часы", categoryId = Guid.NewGuid(), priority = "Normal", variants = Array.Empty<object>(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, badLink.StatusCode);
        Assert.Equal("gifts.link_invalid", await CodeOfAsync(badLink));
        Assert.Equal(HttpStatusCode.BadRequest, badPrice.StatusCode);
        Assert.Equal("gifts.price_invalid", await CodeOfAsync(badPrice));
        Assert.Equal(HttpStatusCode.NotFound, unknownCategory.StatusCode);
        Assert.Equal("gifts.category_not_found", await CodeOfAsync(unknownCategory));
    }

    [SkippableFact]
    public async Task A_Gift_MovesToTheArchive_AndBack()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Колье")));
        var version = (await ItemAsync(client, id)).GetProperty("version").GetUInt32();

        var gifted = await PutAsync(client, $"/api/gifts/items/{id}/status", version, new { status = "Gifted", giftedOn = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, gifted.StatusCode);
        Assert.False(await IsListedAsync(client, id, archived: false));
        var archived = await ItemAsync(client, id, archived: true);
        Assert.Equal("Gifted", archived.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.String, archived.GetProperty("giftedOn").ValueKind);

        var back = await PutAsync(client, $"/api/gifts/items/{id}/status", archived.GetProperty("version").GetUInt32(), new { status = "Wanted" });
        Assert.Equal(HttpStatusCode.NoContent, back.StatusCode);
        var wanted = await ItemAsync(client, id);
        Assert.Equal(JsonValueKind.Null, wanted.GetProperty("giftedOn").ValueKind);

        var dropped = await PutAsync(client, $"/api/gifts/items/{id}/status", wanted.GetProperty("version").GetUInt32(),
            new { status = "Dropped", giftedOn = "2040-01-01" });
        Assert.Equal(HttpStatusCode.NoContent, dropped.StatusCode);
        var droppedItem = await ItemAsync(client, id, archived: true);
        Assert.Equal("Dropped", droppedItem.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, droppedItem.GetProperty("giftedOn").ValueKind);

        var stale = await PutAsync(client, $"/api/gifts/items/{id}/status", version, new { status = "Wanted" });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
    }

    [SkippableFact]
    public async Task The_ActiveList_IsOrdered_ByPriority_ThenTheNearestOccasion()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var someday = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Когда-нибудь", "Someday")));
        var highLate = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Поздний", "High", "2040-05-01")));
        var normal = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Обычный", "Normal")));
        var highUndated = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Без даты", "High")));
        var highSoon = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Ранний", "High", "2040-02-01")));
        Guid[] mine = [someday, highLate, normal, highUndated, highSoon];

        var list = await client.GetFromJsonAsync<JsonElement>("/api/gifts/items");
        var order = list.EnumerateArray().Select(g => g.GetProperty("id").GetGuid()).Where(mine.Contains).ToList();

        Assert.Equal([highSoon, highLate, highUndated, normal, someday], order);
    }
}

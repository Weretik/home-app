using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Gifts.GiftsApi;

namespace IntegrationTests.Api.Gifts;

/// <summary>Gift categories, personal details and the home summary end to end.</summary>
[Collection(ApiCollection.Name)]
public sealed class GiftsSettingsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Starting_Categories_AreSeeded_InOrder()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var categories = await client.GetFromJsonAsync<JsonElement>("/api/gifts/categories");

        Assert.Equal("Техника", categories[0].GetProperty("name").GetString());
        Assert.Equal(Guid.Parse("a1b9482e-bc3b-53cd-8232-0dbe2b4aa249"), categories[0].GetProperty("id").GetGuid());
        Assert.Contains(categories.EnumerateArray(), c => c.GetProperty("name").GetString() == "Другое");
    }

    [SkippableFact]
    public async Task A_Category_IsAdded_Renamed_AndDeleted_OnlyWhileUnused()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var name = $"Книги {Guid.NewGuid():N}";

        var created = await PostAsync(client, "/api/gifts/categories", new { name });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = await IdOfAsync(created);

        var taken = await PostAsync(client, "/api/gifts/categories", new { name = "Техника" });
        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
        Assert.Equal("gifts.category_name_taken", await CodeOfAsync(taken));
        var renamedToTaken = await client.PutAsJsonAsync($"/api/gifts/categories/{id}", new { name = "Техника" });
        Assert.Equal("gifts.category_name_taken", await CodeOfAsync(renamedToTaken));

        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync($"/api/gifts/categories/{id}", new { name = name + " и комиксы" })).StatusCode);
        var gift = await IdOfAsync(await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Vitaliy", name = "Дюна", categoryId = id, priority = "Normal", variants = Array.Empty<object>(),
        }));
        Assert.Equal(name + " и комиксы", (await ItemAsync(client, gift)).GetProperty("categoryName").GetString());

        var inUse = await client.DeleteAsync($"/api/gifts/categories/{id}");
        Assert.Equal(HttpStatusCode.Conflict, inUse.StatusCode);
        Assert.Equal("gifts.category_in_use", await CodeOfAsync(inUse));

        await client.DeleteAsync($"/api/gifts/items/{gift}");
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/gifts/categories/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/gifts/categories/{id}")).StatusCode);
    }

    [SkippableFact]
    public async Task Details_AreAdded_InOrder_Edited_AndDeleted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var ring = await IdOfAsync(await PostAsync(client, "/api/gifts/details", new { recipient = "Maria", label = "Кольцо", value = "18,5–19,0" }));
        var shoes = await IdOfAsync(await PostAsync(client, "/api/gifts/details", new { recipient = "Maria", label = "Обувь", value = "38" }));

        var details = await client.GetFromJsonAsync<JsonElement>("/api/gifts/details");
        var mine = details.EnumerateArray().Where(d => d.GetProperty("id").GetGuid() == ring || d.GetProperty("id").GetGuid() == shoes).ToList();
        Assert.Equal([ring, shoes], mine.Select(d => d.GetProperty("id").GetGuid()));
        var version = mine[1].GetProperty("version").GetUInt32();

        var edited = await PutAsync(client, $"/api/gifts/details/{shoes}", version, new { recipient = "Maria", label = "Обувь", value = "38,5" });
        Assert.Equal(HttpStatusCode.NoContent, edited.StatusCode);
        var stale = await PutAsync(client, $"/api/gifts/details/{shoes}", version, new { recipient = "Maria", label = "Обувь", value = "39" });
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        details = await client.GetFromJsonAsync<JsonElement>("/api/gifts/details");
        Assert.Equal("38,5", details.EnumerateArray().Single(d => d.GetProperty("id").GetGuid() == shoes).GetProperty("value").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/gifts/details/{ring}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/gifts/details/{shoes}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/api/gifts/details/{shoes}")).StatusCode);
    }

    [SkippableFact]
    public async Task The_Summary_CountsPerRecipient_AndListsTopsOccasionsAndDetails()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var before = await client.GetFromJsonAsync<JsonElement>("/api/gifts/summary");
        const string jewellery = "b5691e11-6405-5690-bba9-43e92fef7366";
        var tomorrow = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

        var high = await IdOfAsync(await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Maria", name = "Топ", categoryId = jewellery, priority = "High", price = 1_000m, variants = Array.Empty<object>(),
        }));
        await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Maria", name = "Без цены", categoryId = jewellery, priority = "Normal", variants = Array.Empty<object>(),
        });
        var gifted = await IdOfAsync(await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Maria", name = "Подарено", categoryId = jewellery, priority = "Normal", price = 500m, variants = Array.Empty<object>(),
        }));
        await PutAsync(client, $"/api/gifts/items/{gifted}/status", (await ItemAsync(client, gifted)).GetProperty("version").GetUInt32(), new { status = "Gifted" });
        var together = await IdOfAsync(await PostAsync(client, "/api/gifts/items", new
        {
            recipient = "Together", name = "Скоро", categoryId = jewellery, priority = "Someday", occasion = "Годовщина", occasionDate = tomorrow,
            variants = Array.Empty<object>(),
        }));
        var detail = await IdOfAsync(await PostAsync(client, "/api/gifts/details", new { recipient = "Vitaliy", label = "Рубашка", value = "L" }));

        var after = await client.GetFromJsonAsync<JsonElement>("/api/gifts/summary");

        Assert.Equal(["Maria", "Vitaliy", "Together"], after.GetProperty("recipients").EnumerateArray().Select(r => r.GetProperty("recipient").GetString()));
        var maria = Counts(after, "Maria");
        var mariaBefore = Counts(before, "Maria");
        Assert.Equal(2, maria.GetProperty("wantedCount").GetInt32() - mariaBefore.GetProperty("wantedCount").GetInt32());
        Assert.Equal(1_000m, maria.GetProperty("wantedPriceSum").GetDecimal() - mariaBefore.GetProperty("wantedPriceSum").GetDecimal());
        Assert.Equal(1, maria.GetProperty("wantedWithoutPrice").GetInt32() - mariaBefore.GetProperty("wantedWithoutPrice").GetInt32());
        Assert.Equal(1, maria.GetProperty("highCount").GetInt32() - mariaBefore.GetProperty("highCount").GetInt32());
        Assert.Equal(1, maria.GetProperty("giftedCount").GetInt32() - mariaBefore.GetProperty("giftedCount").GetInt32());
        Assert.Equal(500m, maria.GetProperty("giftedPriceSum").GetDecimal() - mariaBefore.GetProperty("giftedPriceSum").GetDecimal());
        Assert.Equal(
            3,
            after.GetProperty("total").GetProperty("wantedCount").GetInt32() - before.GetProperty("total").GetProperty("wantedCount").GetInt32());

        var mariaTop = after.GetProperty("recipients")[0].GetProperty("topWanted").EnumerateArray().Select(g => g.GetProperty("id").GetGuid());
        Assert.True(mariaTop.Contains(high) || after.GetProperty("recipients")[0].GetProperty("topWanted").GetArrayLength() == 5);
        Assert.Contains(after.GetProperty("upcoming").EnumerateArray(), g => g.GetProperty("id").GetGuid() == together);
        Assert.Contains(after.GetProperty("details").EnumerateArray(), d => d.GetProperty("id").GetGuid() == detail);
    }

    private static JsonElement Counts(JsonElement summary, string recipient) =>
        summary.GetProperty("recipients").EnumerateArray()
            .Single(r => r.GetProperty("recipient").GetString() == recipient)
            .GetProperty("counts");
}

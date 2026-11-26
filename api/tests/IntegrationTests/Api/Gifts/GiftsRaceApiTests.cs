using Gifts.Application.Purchases;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Domain.Results;
using Gifts.Application;
using Gifts.Application.Features.Categories.Commands.DeleteCategory;
using Gifts.Application.Features.Details.Commands.UpdateDetail;
using Gifts.Application.Features.Items.Commands.ChangeGiftStatus;
using Gifts.Application.Features.Items.Commands.UpdateGift;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using IntegrationTests.TestHost;
using Microsoft.Extensions.DependencyInjection;
using static IntegrationTests.Api.Gifts.GiftsApi;

namespace IntegrationTests.Api.Gifts;

/// <summary>
/// The race paths of the Gifts writes: another request changes the same row, or starts using a category, between a
/// handler's read and its save. The handler runs on the real database with <see cref="RacingGiftsDb"/>; the guard
/// must turn the race into a 409 result, never a 500 or a lost update.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class GiftsRaceApiTests(ApiFactory api)
{
    private static object Gift(string name, Guid categoryId, string priority = "Normal") => new
    {
        recipient = "Maria", name, categoryId, priority, variants = Array.Empty<object>(),
    };

    [SkippableFact]
    public async Task A_GiftEdit_RacedByAnotherEdit_IsAConflict_AndTheOtherEditStays()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var category = await FirstCategoryAsync(client);
        var id = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Гонка: правка", category)));
        var version = (await ItemAsync(client, id)).GetProperty("version").GetUInt32();

        try
        {
            using var scope = api.Services.CreateScope();
            var db = new RacingGiftsDb(scope.ServiceProvider.GetRequiredService<IGiftsDbContext>(), async () =>
                Assert.Equal(HttpStatusCode.NoContent,
                    (await PutAsync(client, $"/api/gifts/items/{id}", version, Gift("Гонка: раньше", category, "High"))).StatusCode));

            var result = await new UpdateGiftHandler(db, TimeProvider.System, new GiftLinks("https://web.example")).Handle(
                new UpdateGiftCommand(id, version, Recipient.Vitaliy, "Гонка: позже", category, Priority.Someday, null, null, null, null, null, []),
                CancellationToken.None);

            AssertConflict(result, GiftErrors.ChangedMeanwhile);
            var item = await ItemAsync(client, id);
            Assert.Equal("Гонка: раньше", item.GetProperty("name").GetString());
            Assert.Equal("High", item.GetProperty("priority").GetString());
        }
        finally
        {
            await client.DeleteAsync($"/api/gifts/items/{id}");
        }
    }

    [SkippableFact]
    public async Task A_StatusChange_RacedByAnEdit_IsAConflict_AndTheGiftStaysWanted()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var category = await FirstCategoryAsync(client);
        var id = await IdOfAsync(await PostAsync(client, "/api/gifts/items", Gift("Гонка: статус", category)));
        var version = (await ItemAsync(client, id)).GetProperty("version").GetUInt32();

        try
        {
            using var scope = api.Services.CreateScope();
            var db = new RacingGiftsDb(scope.ServiceProvider.GetRequiredService<IGiftsDbContext>(), async () =>
                Assert.Equal(HttpStatusCode.NoContent,
                    (await PutAsync(client, $"/api/gifts/items/{id}", version, Gift("Гонка: статус, правка", category))).StatusCode));

            var result = await new ChangeGiftStatusHandler(db, TimeProvider.System, new GiftLinks("https://web.example")).Handle(
                new ChangeGiftStatusCommand(id, version, GiftStatus.Gifted, null), CancellationToken.None);

            AssertConflict(result, GiftErrors.ChangedMeanwhile);
            Assert.True(await IsListedAsync(client, id, archived: false));
        }
        finally
        {
            await client.DeleteAsync($"/api/gifts/items/{id}");
        }
    }

    [SkippableFact]
    public async Task A_DetailEdit_RacedByAnotherEdit_IsAConflict_AndTheOtherEditStays()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await IdOfAsync(await PostAsync(client, "/api/gifts/details", new { recipient = "Vitaliy", label = "Гонка", value = "M" }));
        var version = (await client.GetFromJsonAsync<JsonElement>("/api/gifts/details")).EnumerateArray()
            .Single(d => d.GetProperty("id").GetGuid() == id).GetProperty("version").GetUInt32();

        try
        {
            using var scope = api.Services.CreateScope();
            var db = new RacingGiftsDb(scope.ServiceProvider.GetRequiredService<IGiftsDbContext>(), async () =>
                Assert.Equal(HttpStatusCode.NoContent,
                    (await PutAsync(client, $"/api/gifts/details/{id}", version, new { recipient = "Vitaliy", label = "Гонка", value = "L" })).StatusCode));

            var result = await new UpdateDetailHandler(db).Handle(
                new UpdateDetailCommand(id, version, Recipient.Vitaliy, "Гонка", "XL"), CancellationToken.None);

            AssertConflict(result, DetailErrors.ChangedMeanwhile);
            var detail = (await client.GetFromJsonAsync<JsonElement>("/api/gifts/details")).EnumerateArray()
                .Single(d => d.GetProperty("id").GetGuid() == id);
            Assert.Equal("L", detail.GetProperty("value").GetString());
        }
        finally
        {
            await client.DeleteAsync($"/api/gifts/details/{id}");
        }
    }

    [SkippableFact]
    public async Task A_CategoryDelete_RacedByANewGiftInIt_IsAConflict_AndTheCategoryStays()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var category = await IdOfAsync(await PostAsync(client, "/api/gifts/categories", new { name = $"Гонка {Guid.NewGuid():N}" }));
        Guid? gift = null;

        try
        {
            using var scope = api.Services.CreateScope();
            var db = new RacingGiftsDb(scope.ServiceProvider.GetRequiredService<IGiftsDbContext>(), async () =>
            {
                var created = await PostAsync(client, "/api/gifts/items", Gift("Гонка: категория", category));
                Assert.Equal(HttpStatusCode.Created, created.StatusCode);
                gift = await IdOfAsync(created);
            });

            var result = await new DeleteCategoryHandler(db).Handle(new DeleteCategoryCommand(category), CancellationToken.None);

            AssertConflict(result, CategoryErrors.InUse);
            var categories = await client.GetFromJsonAsync<JsonElement>("/api/gifts/categories");
            Assert.Contains(categories.EnumerateArray(), c => c.GetProperty("id").GetGuid() == category);
        }
        finally
        {
            if (gift is { } id)
                await client.DeleteAsync($"/api/gifts/items/{id}");
            await client.DeleteAsync($"/api/gifts/categories/{category}");
        }
    }

    private static async Task<Guid> FirstCategoryAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/gifts/categories"))[0].GetProperty("id").GetGuid();

    private static void AssertConflict(Result result, Error expected)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(expected.Code, result.Error.Code);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using static IntegrationTests.Api.Notes.NoteRequests;

namespace IntegrationTests.Api.Notes;

/// <summary>Categories end to end: the seed, unique names, rename and delete that keep their notes (R-006, R-011, SC-005).</summary>
[Collection(ApiCollection.Name)]
public sealed class NotesCategoriesApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Seeded_Categories_AreListed_InOrder()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var categories = await client.GetFromJsonAsync<JsonElement>("/api/notes/categories");
        var names = categories.EnumerateArray().Select(c => c.GetProperty("name").GetString()).ToList();

        Assert.Equal(["Їжа", "Напої", "Квіти", "Техніка", "Одяг", "Дозвілля", "Подорожі", "Здоров'я", "Дім", "Інше"], names.Take(10));
        Assert.Equal("🍲", categories[0].GetProperty("emoji").GetString());
        Assert.Equal(10, categories[0].GetProperty("sortOrder").GetInt32());
    }

    [SkippableFact]
    public async Task A_Category_IsCreated_Renamed_AndDeleted_ItsNotesStay_WithoutACategory()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();
        var name = $"Книги {tag}";

        var created = await PostAsync(client, "/api/notes/categories", new { name, emoji = "📚" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var categoryId = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var taken = await PostAsync(client, "/api/notes/categories", new { name = name.ToUpperInvariant() });
        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
        Assert.Equal("notes.category_name_taken", await CodeAsync(taken));

        var noteId = await CreateNoteAsync(client, $"Детективи {tag}", new { categoryId });
        await CreateNoteAsync(client, $"Аа без категорії {tag}");
        var category = await CategoryAsync(client, categoryId);
        Assert.Equal(1, category.GetProperty("noteCount").GetInt32());
        Assert.Equal(LastSortOrder(await client.GetFromJsonAsync<JsonElement>("/api/notes/categories")), category.GetProperty("sortOrder").GetInt32());

        var version = category.GetProperty("version").GetUInt32();
        var renamed = await SendAsync(client, HttpMethod.Put, $"/api/notes/categories/{categoryId}", version, new { name = $"{name} і комікси", emoji = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, renamed.StatusCode);
        Assert.Equal($"{name} і комікси", (await FindAsync(client, noteId, tag)).GetProperty("categoryName").GetString());

        var toFood = await SendAsync(client, HttpMethod.Put, $"/api/notes/categories/{categoryId}", (await CategoryAsync(client, categoryId)).GetProperty("version").GetUInt32(), new { name = "їжа" });
        Assert.Equal(HttpStatusCode.Conflict, toFood.StatusCode);
        Assert.Equal("notes.category_name_taken", await CodeAsync(toFood));

        var stale = await SendAsync(client, HttpMethod.Delete, $"/api/notes/categories/{categoryId}", version);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal("notes.changed_meanwhile", await CodeAsync(stale));

        var current = (await CategoryAsync(client, categoryId)).GetProperty("version").GetUInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await SendAsync(client, HttpMethod.Delete, $"/api/notes/categories/{categoryId}", current)).StatusCode);

        var note = await FindAsync(client, noteId, tag);
        Assert.Equal(JsonValueKind.Null, note.GetProperty("categoryId").ValueKind);
        Assert.Equal(JsonValueKind.Null, note.GetProperty("categoryName").ValueKind);
        // Without a category the notes go last, by title.
        Assert.Equal([$"Аа без категорії {tag}", $"Детективи {tag}"], Titles(await ListAsync(client, $"search={tag}")));
        var missing = await SendAsync(client, HttpMethod.Delete, $"/api/notes/categories/{categoryId}", current);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("notes.category_not_found", await CodeAsync(missing));
    }

    [SkippableFact]
    public void Every_Write_OfTheModule_IsRateLimited()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);

        var writes = api.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText?.StartsWith("/api/notes", StringComparison.Ordinal) == true)
            .Where(e => e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Any(m => m != HttpMethods.Get) == true)
            .ToList();

        Assert.Equal(6, writes.Count);
        Assert.All(writes, e => Assert.NotNull(e.Metadata.GetMetadata<EnableRateLimitingAttribute>()));
    }

    private static int LastSortOrder(JsonElement categories) =>
        categories.EnumerateArray().Max(c => c.GetProperty("sortOrder").GetInt32());

    private static async Task<JsonElement> CategoryAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/notes/categories"))
            .EnumerateArray().Single(c => c.GetProperty("id").GetGuid() == id);
}

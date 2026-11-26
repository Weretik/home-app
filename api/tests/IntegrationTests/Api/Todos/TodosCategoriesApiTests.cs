using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using static IntegrationTests.Api.Todos.TodoRequests;

namespace IntegrationTests.Api.Todos;

/// <summary>Categories end to end: the seed, unique names, rename and delete that keep their tasks.</summary>
[Collection(ApiCollection.Name)]
public sealed class TodosCategoriesApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Seeded_Categories_AreListed_InOrder()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var categories = await client.GetFromJsonAsync<JsonElement>("/api/todos/categories");
        var names = categories.EnumerateArray().Select(c => c.GetProperty("name").GetString()).ToList();

        Assert.Equal(["Дом", "Ремонт", "Здоровье", "Работа", "Финансы", "Документы", "Покупки", "Отдых"], names.Take(8));
        Assert.Equal("🏠", categories[0].GetProperty("emoji").GetString());
    }

    [SkippableFact]
    public async Task A_Category_IsCreated_Renamed_AndDeleted_ItsTasksStay()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var name = $"Дача {TodosTasksApiTests.Tag()}";

        var created = await PostAsync(client, "/api/todos/categories", new { name, emoji = "🏡" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var categoryId = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var taken = await PostAsync(client, "/api/todos/categories", new { name = name.ToUpperInvariant() });
        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
        Assert.Equal("todos.category_name_taken", await CodeAsync(taken));

        var taskId = await CreateTaskAsync(client, $"Покосить траву {TodosTasksApiTests.Tag()}", new { categoryId });
        var category = await FindAsync(client, categoryId);
        Assert.Equal(1, category.GetProperty("activeCount").GetInt32());
        Assert.Equal(LastSortOrder(await client.GetFromJsonAsync<JsonElement>("/api/todos/categories")), category.GetProperty("sortOrder").GetInt32());

        var version = category.GetProperty("version").GetUInt32();
        var renamed = await SendAsync(client, HttpMethod.Put, $"/api/todos/categories/{categoryId}", version, new { name = $"{name} у озера", emoji = (string?)null });
        Assert.Equal(HttpStatusCode.NoContent, renamed.StatusCode);
        Assert.Equal($"{name} у озера", (await GetTaskAsync(client, taskId)).GetProperty("categoryName").GetString());

        var toHome = await SendAsync(client, HttpMethod.Put, $"/api/todos/categories/{categoryId}", (await FindAsync(client, categoryId)).GetProperty("version").GetUInt32(), new { name = "дом" });
        Assert.Equal(HttpStatusCode.Conflict, toHome.StatusCode);
        Assert.Equal("todos.category_name_taken", await CodeAsync(toHome));

        var stale = await SendAsync(client, HttpMethod.Delete, $"/api/todos/categories/{categoryId}", version);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);

        var current = (await FindAsync(client, categoryId)).GetProperty("version").GetUInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await SendAsync(client, HttpMethod.Delete, $"/api/todos/categories/{categoryId}", current)).StatusCode);

        var task = await GetTaskAsync(client, taskId);
        Assert.Equal(JsonValueKind.Null, task.GetProperty("categoryId").ValueKind);
        Assert.Equal(JsonValueKind.Null, task.GetProperty("categoryName").ValueKind);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(client, HttpMethod.Delete, $"/api/todos/categories/{categoryId}", current)).StatusCode);
    }

    [SkippableFact]
    public void Every_Write_OfTheModule_IsRateLimited()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);

        var writes = api.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText?.StartsWith("/api/todos", StringComparison.Ordinal) == true)
            .Where(e => e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Any(m => m != HttpMethods.Get) == true)
            .ToList();

        Assert.Equal(10, writes.Count);
        Assert.All(writes, e => Assert.NotNull(e.Metadata.GetMetadata<EnableRateLimitingAttribute>()));
    }

    private static int LastSortOrder(JsonElement categories) =>
        categories.EnumerateArray().Max(c => c.GetProperty("sortOrder").GetInt32());

    private static async Task<JsonElement> FindAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/todos/categories"))
            .EnumerateArray().Single(c => c.GetProperty("id").GetGuid() == id);
}

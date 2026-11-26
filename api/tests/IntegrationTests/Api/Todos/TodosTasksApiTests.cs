using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Todos.TodoRequests;

namespace IntegrationTests.Api.Todos;

/// <summary>
/// Tasks end to end: create, list and filter, archive and reopen, versions and repeats. Each test puts a word of its
/// own into its titles and searches by it, so tests sharing the database do not see each other's tasks.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class TodosTasksApiTests(ApiFactory api)
{
    private const string Home = "0193b6a0-0000-7000-8000-000000000010";

    [SkippableFact]
    public async Task A_Task_IsCreated_Listed_Archived_AndReopened()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        var id = await CreateTaskAsync(client, $"Приклеить деревяшки {tag}", new
        {
            notes = "  клей в кладовке  ",
            author = "Vitaliy",
            priority = "High",
            dueDate = "2026-11-30",
            categoryId = Home,
        });

        var item = await GetTaskAsync(client, id);
        Assert.Equal("клей в кладовке", item.GetProperty("notes").GetString());
        Assert.Equal("Todo", item.GetProperty("status").GetString());
        Assert.Equal("Дом", item.GetProperty("categoryName").GetString());
        Assert.Equal("🏠", item.GetProperty("categoryEmoji").GetString());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("completedAtUtc").ValueKind);

        Assert.Contains($"Приклеить деревяшки {tag}", Titles(await ListAsync(client, $"search={tag}&assignee=Maria")));
        Assert.Empty(Titles(await ListAsync(client, $"search={tag}&assignee=Shared")));

        var version = item.GetProperty("version").GetUInt32();
        var done = await SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{id}/status", version, new { status = "Done" });
        Assert.Equal(HttpStatusCode.NoContent, done.StatusCode);
        Assert.Empty(Titles(await ListAsync(client, $"search={tag}")));
        var archived = await ListAsync(client, $"view=archive&search={tag}");
        Assert.Equal(JsonValueKind.String, archived.GetProperty("items")[0].GetProperty("completedAtUtc").ValueKind);

        item = await GetTaskAsync(client, id);
        var reopened = await SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{id}/status", item.GetProperty("version").GetUInt32(), new { status = "Todo" });
        Assert.Equal(HttpStatusCode.NoContent, reopened.StatusCode);
        item = await GetTaskAsync(client, id);
        Assert.Equal(JsonValueKind.Null, item.GetProperty("completedAtUtc").ValueKind);
        Assert.Single(Titles(await ListAsync(client, $"search={tag}")));
    }

    [SkippableFact]
    public async Task The_Active_List_IsOrdered_ByPriority_ThenDueDate_ThenCreation()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        await CreateTaskAsync(client, $"low {tag}", new { priority = "Low", dueDate = "2026-01-01" });
        await CreateTaskAsync(client, $"normal-nodate {tag}");
        await CreateTaskAsync(client, $"normal-late {tag}", new { dueDate = "2027-01-01" });
        await CreateTaskAsync(client, $"normal-early {tag}", new { dueDate = "2026-12-01" });
        await CreateTaskAsync(client, $"urgent {tag}", new { priority = "Urgent" });

        var titles = Titles(await ListAsync(client, $"search={tag}")).Select(t => t.Split(' ')[0]);

        Assert.Equal(["urgent", "normal-early", "normal-late", "normal-nodate", "low"], titles);
    }

    [SkippableFact]
    public async Task The_Archive_IsOrdered_ByCompletion_NewestFirst()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();
        var first = await CreateTaskAsync(client, $"first {tag}");
        var second = await CreateTaskAsync(client, $"second {tag}");
        var third = await CreateTaskAsync(client, $"third {tag}");

        // Archived in the order second, first, third (one cancelled), so creation order cannot explain the result.
        foreach (var (id, status) in new[] { (second, "Done"), (first, "Cancelled"), (third, "Done") })
        {
            var version = (await GetTaskAsync(client, id)).GetProperty("version").GetUInt32();
            var moved = await SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{id}/status", version, new { status });
            Assert.Equal(HttpStatusCode.NoContent, moved.StatusCode);
        }

        var titles = Titles(await ListAsync(client, $"view=archive&search={tag}")).Select(t => t.Split(' ')[0]);

        Assert.Equal(["third", "first", "second"], titles);
    }

    [SkippableFact]
    public async Task Filters_Combine_Assignees_Categories_AndSearchWords()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        await CreateTaskAsync(client, $"Разобрать балкон {tag}", new { assignee = "Maria", categoryId = Home });
        await CreateTaskAsync(client, $"Позвонить маме {tag}", new { assignee = "Shared" });
        await CreateTaskAsync(client, $"Поменять масло {tag}", new { assignee = "Vitaliy" });

        Assert.Equal(2, (await ListAsync(client, $"search={tag}&assignee=Maria&assignee=Shared")).GetProperty("totalCount").GetInt32());
        Assert.Equal([$"Разобрать балкон {tag}"], Titles(await ListAsync(client, $"search={tag}&categoryId={Home}")));
        Assert.Equal(2, (await ListAsync(client, $"search={tag}&withoutCategory=true")).GetProperty("totalCount").GetInt32());
        Assert.Equal([$"Разобрать балкон {tag}"], Titles(await ListAsync(client, $"search=БАЛКОН%20{tag}")));
        Assert.Empty(Titles(await ListAsync(client, $"search=%25%20{tag}")));

        var bad = await client.GetAsync("/api/todos/tasks?view=trash");
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
    }

    [SkippableFact]
    public async Task A_Stale_Version_IsAConflict_ForEditStatusAndDelete()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await CreateTaskAsync(client, $"Версия {Tag()}");
        var stale = (await GetTaskAsync(client, id)).GetProperty("version").GetUInt32();

        var edit = await SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{id}", stale, Fields("Первая правка"));
        Assert.Equal(HttpStatusCode.NoContent, edit.StatusCode);

        var second = await SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{id}", stale, Fields("Вторая правка"));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("todos.changed_meanwhile", await CodeAsync(second));
        Assert.Equal(HttpStatusCode.Conflict, (await SendAsync(client, HttpMethod.Put, $"/api/todos/tasks/{id}/status", stale, new { status = "Done" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await SendAsync(client, HttpMethod.Delete, $"/api/todos/tasks/{id}", stale)).StatusCode);

        var noVersion = await client.PutAsJsonAsync($"/api/todos/tasks/{id}", Fields("Без версии"));
        Assert.Equal(HttpStatusCode.BadRequest, noVersion.StatusCode);

        var current = (await GetTaskAsync(client, id)).GetProperty("version").GetUInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await SendAsync(client, HttpMethod.Delete, $"/api/todos/tasks/{id}", current)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/todos/tasks/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await SendAsync(client, HttpMethod.Delete, $"/api/todos/tasks/{id}", current)).StatusCode);
    }

    [SkippableFact]
    public async Task A_Repeated_Create_ReturnsTheFirstId_AndOtherData_IsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var key = Guid.NewGuid().ToString();
        var tag = Tag();
        var body = new { title = $"Один раз {tag}", assignee = "Shared", priority = "Normal", steps = new[] { "а", "б" } };

        var first = await PostAsync(client, "/api/todos/tasks", body, key);
        var repeat = await PostAsync(client, "/api/todos/tasks", body, key);
        var other = await PostAsync(client, "/api/todos/tasks", body with { title = $"Другое {tag}" }, key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
        Assert.Equal(
            (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid(),
            (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        Assert.Equal(1, (await ListAsync(client, $"search={tag}")).GetProperty("totalCount").GetInt32());

        var missingKey = await PostAsync(client, "/api/todos/tasks", body);
        Assert.Equal(HttpStatusCode.BadRequest, missingKey.StatusCode);
        var unknownCategory = await PostAsync(client, "/api/todos/tasks", new { title = "x", assignee = "Shared", priority = "Normal", categoryId = Guid.NewGuid() }, Guid.NewGuid().ToString());
        Assert.Equal(HttpStatusCode.NotFound, unknownCategory.StatusCode);
        Assert.Equal("todos.category_not_found", await CodeAsync(unknownCategory));
    }

    internal static string Tag() => "t" + Guid.NewGuid().ToString("N")[..10];

    private static Task<JsonElement> ListAsync(HttpClient client, string query) =>
        client.GetFromJsonAsync<JsonElement>($"/api/todos/tasks?{query}");

    private static object Fields(string title) =>
        new { title, assignee = "Vitaliy", status = "InProgress", priority = "Urgent", dueDate = (string?)null, categoryId = (Guid?)null };
}

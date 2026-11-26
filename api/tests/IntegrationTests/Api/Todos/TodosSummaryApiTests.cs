using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Todos.TodoRequests;

namespace IntegrationTests.Api.Todos;

/// <summary>The home page summary: counts per assignee for today in Kyiv, and the tasks in focus.</summary>
[Collection(ApiCollection.Name)]
public sealed class TodosSummaryApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Summary_CountsActiveOverdueAndToday_PerAssignee()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var before = await client.GetFromJsonAsync<JsonElement>("/api/todos/summary");
        var today = DateOnly.Parse(before.GetProperty("today").GetString()!);
        Assert.Equal(["Maria", "Vitaliy", "Shared"], before.GetProperty("assignees").EnumerateArray().Select(a => a.GetProperty("assignee").GetString()));

        var tag = TodosTasksApiTests.Tag();
        var overdue = await CreateTaskAsync(client, $"Просрочено {tag}", new { assignee = "Shared", dueDate = today.AddDays(-2).ToString("O") });
        await CreateTaskAsync(client, $"Сегодня {tag}", new { assignee = "Shared", dueDate = today.ToString("O") });
        await CreateTaskAsync(client, $"Потом {tag}", new { assignee = "Shared", dueDate = today.AddDays(5).ToString("O") });
        await CreateTaskAsync(client, $"Сделано {tag}", new { assignee = "Shared", status = "Done", dueDate = today.AddDays(-1).ToString("O") });

        var after = await client.GetFromJsonAsync<JsonElement>("/api/todos/summary");

        var shared = (Before: Counts(before, "Shared"), After: Counts(after, "Shared"));
        Assert.Equal(shared.Before.Active + 3, shared.After.Active);
        Assert.Equal(shared.Before.Overdue + 1, shared.After.Overdue);
        Assert.Equal(shared.Before.DueToday + 1, shared.After.DueToday);
        Assert.Equal(Counts(before, "Maria"), Counts(after, "Maria"));
        Assert.Equal(before.GetProperty("doneLast7Days").GetInt32() + 1, after.GetProperty("doneLast7Days").GetInt32());

        var focus = after.GetProperty("focus").EnumerateArray().ToList();
        Assert.InRange(focus.Count, 1, 8);
        Assert.Contains(focus, t => t.GetProperty("id").GetGuid() == overdue);
        Assert.DoesNotContain(focus, t => t.GetProperty("title").GetString() == $"Потом {tag}");
        Assert.DoesNotContain(focus, t => t.GetProperty("title").GetString() == $"Сделано {tag}");
    }

    [SkippableFact]
    public async Task The_Focus_HoldsAtMostEight_TheOldestOverdueFirst()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = TodosTasksApiTests.Tag();

        // Ten overdue tasks dated at the very start of the allowed range: older than any other test's tasks.
        var ids = new List<Guid>();
        foreach (var day in Enumerable.Range(1, 10).Reverse())
            ids.Add(await CreateTaskAsync(client, $"Давно {day:00} {tag}", new { priority = "Low", dueDate = new DateOnly(2000, 1, day).ToString("O") }));

        try
        {
            var focus = (await client.GetFromJsonAsync<JsonElement>("/api/todos/summary")).GetProperty("focus")
                .EnumerateArray().Select(t => t.GetProperty("title").GetString()).ToList();

            Assert.Equal(Enumerable.Range(1, 8).Select(day => $"Давно {day:00} {tag}"), focus);
        }
        finally
        {
            // The focus is shared by every test: leave it as it was.
            foreach (var id in ids)
                await SendAsync(client, HttpMethod.Delete, $"/api/todos/tasks/{id}", (await GetTaskAsync(client, id)).GetProperty("version").GetUInt32());
        }
    }

    private static (int Active, int Overdue, int DueToday) Counts(JsonElement summary, string assignee)
    {
        var counts = summary.GetProperty("assignees").EnumerateArray().Single(a => a.GetProperty("assignee").GetString() == assignee);
        return (counts.GetProperty("active").GetInt32(), counts.GetProperty("overdue").GetInt32(), counts.GetProperty("dueToday").GetInt32());
    }
}

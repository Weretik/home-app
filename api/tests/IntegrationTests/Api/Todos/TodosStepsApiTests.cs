using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Todos.TodoRequests;

namespace IntegrationTests.Api.Todos;

/// <summary>Steps of a task end to end: they need no version, yet every change moves the task's version.</summary>
[Collection(ApiCollection.Name)]
public sealed class TodosStepsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Steps_AreAdded_Ticked_AndDeleted_InOrder()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await CreateTaskAsync(client, $"Шаги {TodosTasksApiTests.Tag()}", new { steps = new[] { "Купить клей", "Приклеить" } });
        var before = (await GetTaskAsync(client, id)).GetProperty("version").GetUInt32();

        var added = await PostAsync(client, $"/api/todos/tasks/{id}/steps", new { title = "Покрасить" });
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);

        var task = await GetTaskAsync(client, id);
        var steps = task.GetProperty("steps");
        Assert.Equal(["Купить клей", "Приклеить", "Покрасить"], steps.EnumerateArray().Select(s => s.GetProperty("title").GetString()));
        Assert.NotEqual(before, task.GetProperty("version").GetUInt32());

        foreach (var index in new[] { 0, 1 })
        {
            var step = steps[index];
            var ticked = await client.PutAsJsonAsync(
                $"/api/todos/tasks/{id}/steps/{step.GetProperty("id").GetGuid()}",
                new { title = step.GetProperty("title").GetString(), isDone = true });
            Assert.Equal(HttpStatusCode.NoContent, ticked.StatusCode);
        }

        task = await GetTaskAsync(client, id);
        Assert.Equal(2, task.GetProperty("stepsDone").GetInt32());
        Assert.Equal(3, task.GetProperty("stepsTotal").GetInt32());

        var deleted = await client.DeleteAsync($"/api/todos/tasks/{id}/steps/{steps[2].GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        task = await GetTaskAsync(client, id);
        Assert.Equal(2, task.GetProperty("stepsDone").GetInt32());
        Assert.Equal(2, task.GetProperty("stepsTotal").GetInt32());

        var missing = await client.DeleteAsync($"/api/todos/tasks/{id}/steps/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("todos.step_not_found", await CodeAsync(missing));
    }

    [SkippableFact]
    public async Task Ticking_Steps_AtTheSameTime_NeverConflicts()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var titles = Enumerable.Range(1, 8).Select(i => $"шаг {i}").ToArray();
        var id = await CreateTaskAsync(client, $"Быстрые галочки {TodosTasksApiTests.Tag()}", new { steps = titles });
        var steps = (await GetTaskAsync(client, id)).GetProperty("steps").EnumerateArray().ToList();

        // R-010: steps need no version, so ticks sent together from two phones must all land.
        var ticks = await Task.WhenAll(steps.Select(step => client.PutAsJsonAsync(
            $"/api/todos/tasks/{id}/steps/{step.GetProperty("id").GetGuid()}",
            new { title = step.GetProperty("title").GetString(), isDone = true })));

        Assert.All(ticks, tick => Assert.Equal(HttpStatusCode.NoContent, tick.StatusCode));
        Assert.Equal(titles.Length, (await GetTaskAsync(client, id)).GetProperty("stepsDone").GetInt32());
    }

    [SkippableFact]
    public async Task A_Task_HoldsAtMost50Steps_AndAnUnknownTask_IsNotFound()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await CreateTaskAsync(client, $"Много шагов {TodosTasksApiTests.Tag()}", new { steps = Enumerable.Range(1, 50).Select(i => $"шаг {i}").ToArray() });

        var tooMany = await PostAsync(client, $"/api/todos/tasks/{id}/steps", new { title = "51" });
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
        Assert.Equal("todos.too_many_steps", await CodeAsync(tooMany));

        var noTask = await PostAsync(client, $"/api/todos/tasks/{Guid.NewGuid()}/steps", new { title = "x" });
        Assert.Equal(HttpStatusCode.NotFound, noTask.StatusCode);
        Assert.Equal("todos.task_not_found", await CodeAsync(noTask));
    }

    [SkippableFact]
    public async Task Deleting_ATask_DeletesItsSteps()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var id = await CreateTaskAsync(client, $"С шагами {TodosTasksApiTests.Tag()}", new { steps = new[] { "a" } });
        var version = (await GetTaskAsync(client, id)).GetProperty("version").GetUInt32();

        var deleted = await SendAsync(client, HttpMethod.Delete, $"/api/todos/tasks/{id}", version);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/todos/tasks/{id}")).StatusCode);
    }
}

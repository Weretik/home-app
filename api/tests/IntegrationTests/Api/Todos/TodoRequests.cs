using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Api.Todos;

/// <summary>The requests the Todos tests repeat: creates with an Idempotency-Key, edits with If-Match, reads by id.</summary>
internal static class TodoRequests
{
    public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body, string? key = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        if (key is not null)
            request.Headers.Add("Idempotency-Key", key);
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string url, uint version, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }

    /// <summary>Creates a task and returns its id; the fields not given are those of a plain task for Маша.</summary>
    public static async Task<Guid> CreateTaskAsync(HttpClient client, string title, object? fields = null)
    {
        var body = new Dictionary<string, object?>
        {
            ["title"] = title,
            ["assignee"] = "Maria",
            ["priority"] = "Normal",
        };
        if (fields is not null)
        {
            foreach (var property in JsonSerializer.SerializeToElement(fields).EnumerateObject())
                body[property.Name] = property.Value;
        }

        var response = await PostAsync(client, "/api/todos/tasks", body, Guid.NewGuid().ToString());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    public static Task<JsonElement> GetTaskAsync(HttpClient client, Guid id) =>
        client.GetFromJsonAsync<JsonElement>($"/api/todos/tasks/{id}");

    public static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

    public static IEnumerable<string> Titles(JsonElement list) =>
        list.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("title").GetString()!);
}

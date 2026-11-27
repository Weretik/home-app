using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api.Notes;

/// <summary>
/// The requests the Notes tests repeat: creates with an Idempotency-Key, edits with If-Match, the list, and a client
/// signed in as Masha. Each test puts a tag of its own into its titles and searches by it, so tests sharing the
/// database do not see each other's notes.
/// </summary>
internal static class NoteRequests
{
    public const string Food = "019ac3a0-0000-7000-8000-000000000010";
    public const string Flowers = "019ac3a0-0000-7000-8000-000000000030";

    public static string Tag() => "n" + Guid.NewGuid().ToString("N")[..10];

    public static async Task<HttpClient> MashaAsync(ApiFactory api)
    {
        var client = api.CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await api.SignInAsync(ApiFactory.Masha));
        return client;
    }

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

    /// <summary>Creates a note and returns its id; the fields not given are those of a plain «Likes» note about Маша.</summary>
    public static async Task<Guid> CreateNoteAsync(HttpClient client, string title, object? fields = null)
    {
        var response = await PostAsync(client, "/api/notes/items", Body(title, fields), Guid.NewGuid().ToString());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    public static Dictionary<string, object?> Body(string title, object? fields = null)
    {
        var body = new Dictionary<string, object?>
        {
            ["about"] = "Maria",
            ["kind"] = "Likes",
            ["title"] = title,
            ["isImportant"] = false,
        };
        if (fields is not null)
        {
            foreach (var property in JsonSerializer.SerializeToElement(fields).EnumerateObject())
                body[property.Name] = property.Value;
        }

        return body;
    }

    public static Task<JsonElement> ListAsync(HttpClient client, string query) =>
        client.GetFromJsonAsync<JsonElement>($"/api/notes/items?{query}");

    public static async Task<JsonElement> FindAsync(HttpClient client, Guid id, string tag) =>
        (await ListAsync(client, $"search={tag}")).EnumerateArray().Single(n => n.GetProperty("id").GetGuid() == id);

    public static IEnumerable<string> Titles(JsonElement list) =>
        list.EnumerateArray().Select(n => n.GetProperty("title").GetString()!);

    public static async Task<string?> CodeAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();
}

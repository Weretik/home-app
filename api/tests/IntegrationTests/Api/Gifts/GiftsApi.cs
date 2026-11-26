using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Api.Gifts;

/// <summary>Requests the Gifts tests repeat: creates with an Idempotency-Key, edits with If-Match.</summary>
internal static class GiftsApi
{
    public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body, string? key = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", key ?? Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PutAsync(HttpClient client, string url, uint version, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }

    public static async Task<Guid> IdOfAsync(HttpResponseMessage created) =>
        (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

    public static async Task<JsonElement> ItemAsync(HttpClient client, Guid id, bool archived = false)
    {
        var list = await client.GetFromJsonAsync<JsonElement>($"/api/gifts/items?archived={(archived ? "true" : "false")}");
        return list.EnumerateArray().Single(g => g.GetProperty("id").GetGuid() == id);
    }

    public static async Task<bool> IsListedAsync(HttpClient client, Guid id, bool archived)
    {
        var list = await client.GetFromJsonAsync<JsonElement>($"/api/gifts/items?archived={(archived ? "true" : "false")}");
        return list.EnumerateArray().Any(g => g.GetProperty("id").GetGuid() == id);
    }

    public static async Task<string?> CodeOfAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();
}

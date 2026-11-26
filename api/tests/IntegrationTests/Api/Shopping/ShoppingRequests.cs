using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests.Api.Shopping;

/// <summary>
/// Calls of the Shopping API. The list is one shared list and the tests share the database, so each test names its
/// items with a unique suffix (<see cref="Unique"/>) and looks only at them.
/// </summary>
internal static class ShoppingRequests
{
    public const string Root = "/api/shopping";

    /// <summary>Eight Latin letters: no digits, so the parser never reads the suffix as a quantity.</summary>
    public static string Unique() =>
        new(Guid.NewGuid().ToString("N")[..8].Select(c => char.IsAsciiDigit(c) ? (char)('g' + (c - '0')) : c).ToArray());

    public static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body, string? key = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Root + url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", key ?? Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> PutAsync(HttpClient client, string url, object body, uint? version = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, Root + url) { Content = JsonContent.Create(body) };
        if (version is { } v)
            request.Headers.TryAddWithoutValidation("If-Match", $"\"{v}\"");
        return await client.SendAsync(request);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(HttpClient client, string url, uint version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, Root + url);
        request.Headers.TryAddWithoutValidation("If-Match", version.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return await client.SendAsync(request);
    }

    public static Task<JsonElement> GetAsync(HttpClient client, string url) => client.GetFromJsonAsync<JsonElement>(Root + url);

    /// <summary>Adds text and returns the add result.</summary>
    public static async Task<JsonElement> AddAsync(HttpClient client, string text, string addedBy = "Vitaliy", Guid? departmentId = null, bool urgent = false)
    {
        var response = await PostAsync(client, "/items", new { text, addedBy, departmentId, urgent });
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    /// <summary>The list row with this name, or null.</summary>
    public static async Task<JsonElement?> InListAsync(HttpClient client, string name)
    {
        var list = await GetAsync(client, "/items");
        foreach (var item in list.GetProperty("items").EnumerateArray())
        {
            if (item.GetProperty("name").GetString() == name)
                return item;
        }

        return null;
    }

    public static async Task<JsonElement> DepartmentAsync(HttpClient client, string name)
    {
        var departments = await GetAsync(client, "/departments");
        return departments.EnumerateArray().Single(d => d.GetProperty("name").GetString() == name);
    }

    public static Guid Id(this JsonElement element) => element.GetProperty("id").GetGuid();

    public static uint Version(this JsonElement element) => element.GetProperty("version").GetUInt32();

    public static string Text(this JsonElement element, string property) => element.GetProperty(property).GetString()!;
}

using System.Net;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api;

/// <summary>The web app lives on another origin, so the API must allow it explicitly.</summary>
[Collection(ApiCollection.Name)]
public sealed class CorsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Status_IsReadable_FromTheWebOrigin()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/system/status");
        request.Headers.Add("Origin", ApiFactory.WebOrigin);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ApiFactory.WebOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [SkippableFact]
    public async Task An_UnknownOrigin_GetsNoCorsHeader()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/system/status");
        request.Headers.Add("Origin", "https://evil.example");

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}

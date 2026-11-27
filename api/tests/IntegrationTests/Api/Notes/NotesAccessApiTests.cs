using System.Net;
using System.Net.Http.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Notes.NoteRequests;

namespace IntegrationTests.Api.Notes;

/// <summary>
/// Notes are family data: nothing without a session, the deploy check key only reads (R-014, SC-007). The identity
/// sweeps (<c>IdentityProtectionApiTests</c>) also cover both reads, since they enumerate every GET of the contract.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class NotesAccessApiTests(ApiFactory api)
{
    public static TheoryData<string, string> Operations => new()
    {
        { "GET", "/api/notes/items" },
        { "POST", "/api/notes/items" },
        { "PUT", "/api/notes/items/0195d2c4-0000-7000-8000-000000000001" },
        { "DELETE", "/api/notes/items/0195d2c4-0000-7000-8000-000000000001" },
        { "GET", "/api/notes/categories" },
        { "POST", "/api/notes/categories" },
        { "PUT", "/api/notes/categories/0195d2c4-0000-7000-8000-000000000001" },
        { "DELETE", "/api/notes/categories/0195d2c4-0000-7000-8000-000000000001" },
    };

    [SkippableTheory]
    [MemberData(nameof(Operations))]
    public async Task Without_ASession_EveryOperation_Answers401(string method, string path)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(Body("Чай"));

        var response = await anonymous.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task The_DeployCheckKey_ReadsTheNotes_ButCannotCreateOne()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var deploy = api.CreateAnonymousClient();
        deploy.DefaultRequestHeaders.Add("X-Deploy-Check-Key", ApiFactory.DeployCheckKey);

        Assert.Equal(HttpStatusCode.OK, (await deploy.GetAsync("/api/notes/items")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await deploy.GetAsync("/api/notes/categories")).StatusCode);
        var write = await PostAsync(deploy, "/api/notes/items", Body($"Чай {Tag()}"), Guid.NewGuid().ToString());
        Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
    }
}

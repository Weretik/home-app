using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Application.Authentication;
using Identity.Domain.Sessions;
using Identity.Infrastructure;
using IntegrationTests.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Api.Identity;

/// <summary>Sign-in, sign-out, the current member and the session rules (spec identity/001, R-001…R-008, R-012).</summary>
[Collection(ApiCollection.Name)]
public sealed class IdentityApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task A_Member_SignsIn_AndTheTokenOpensTheApi()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();

        var response = await anonymous.PostAsJsonAsync("/api/identity/sign-in", new { credential = FakeGoogleVerifier.Credential(" Masha@Family.test ") });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(ApiFactory.Masha, body.GetProperty("member").GetProperty("email").GetString());
        Assert.Equal("Maria", body.GetProperty("member").GetProperty("person").GetString());
        Assert.True(body.GetProperty("expiresAtUtc").GetDateTimeOffset() > DateTimeOffset.UtcNow.AddDays(29));
        using var masha = WithToken(body.GetProperty("token").GetString()!);
        var me = await masha.GetFromJsonAsync<JsonElement>("/api/identity/me");
        Assert.Equal(ApiFactory.Masha, me.GetProperty("email").GetString());
        Assert.Contains("Masha", me.GetProperty("name").GetString());
        Assert.Equal("Maria", me.GetProperty("person").GetString());
        Assert.Equal(HttpStatusCode.OK, (await masha.GetAsync("/api/todos/summary")).StatusCode);
    }

    [SkippableFact]
    public async Task Me_SaysWhoTheMemberIsInTheHome()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var vitaliy = api.CreateHttpsClient();

        var me = await vitaliy.GetFromJsonAsync<JsonElement>("/api/identity/me");

        Assert.Equal(ApiFactory.Vitaliy, me.GetProperty("email").GetString());
        Assert.Equal("Vitaliy", me.GetProperty("person").GetString());
    }

    [SkippableFact]
    public async Task A_Stranger_IsRefused_AndNothingIsStored()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();

        var response = await anonymous.PostAsJsonAsync("/api/identity/sign-in", new { credential = FakeGoogleVerifier.Credential("stranger@example.com") });

        await AssertProblemAsync(response, HttpStatusCode.Forbidden, "identity.not_a_member");
        Assert.Equal(0, await CountSessionsAsync("stranger@example.com"));
    }

    [SkippableTheory]
    [InlineData("not-a-google-token")]
    [InlineData("google-unverified:vitaliy@family.test")]
    public async Task A_TokenGoogleDidNotConfirm_IsRefused(string credential)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();

        var response = await anonymous.PostAsJsonAsync("/api/identity/sign-in", new { credential });

        await AssertProblemAsync(response, HttpStatusCode.Unauthorized, "identity.google_token_invalid");
    }

    [SkippableFact]
    public async Task An_EmptyCredential_IsAValidationProblem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var anonymous = api.CreateAnonymousClient();

        var response = await anonymous.PostAsJsonAsync("/api/identity/sign-in", new { credential = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [SkippableFact]
    public async Task Sign_Out_EndsOnlyThatSession()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var other = WithToken(await api.SignInAsync(ApiFactory.Vitaliy));
        using var phone = WithToken(await api.SignInAsync(ApiFactory.Vitaliy));
        Assert.Equal(HttpStatusCode.OK, (await phone.GetAsync("/api/identity/me")).StatusCode);

        var signOut = await phone.PostAsync("/api/identity/sign-out", null);

        Assert.Equal(HttpStatusCode.NoContent, signOut.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await phone.GetAsync("/api/identity/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await phone.PostAsync("/api/identity/sign-out", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await other.GetAsync("/api/identity/me")).StatusCode);
    }

    [SkippableFact]
    public async Task An_ExpiredSession_IsRefused_AndAnActiveOne_Slides()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        var now = DateTimeOffset.UtcNow;
        var expired = await StoreSessionAsync(ApiFactory.Masha, now.AddDays(-31));
        var old = await StoreSessionAsync(ApiFactory.Masha, now.AddDays(-20));

        using var expiredClient = WithToken(expired);
        using var oldClient = WithToken(old);

        Assert.Equal(HttpStatusCode.Unauthorized, (await expiredClient.GetAsync("/api/gifts/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await oldClient.GetAsync("/api/gifts/summary")).StatusCode);
        var slid = await FindSessionAsync(old);
        Assert.True(slid.ExpiresAtUtc > now.AddDays(29), $"expiry {slid.ExpiresAtUtc} did not slide");
    }

    [SkippableFact]
    public async Task A_Member_RemovedFromTheList_IsLockedOut_AtOnce()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        var masha = await api.SignInAsync(ApiFactory.Masha);
        await using var host = api.WithWebHostBuilder(builder => builder.UseSetting("Identity:AllowedEmails:1", ""));
        using var client = host.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", masha);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/todos/summary")).StatusCode);
        api.SignedIn(client);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/todos/summary")).StatusCode);
    }

    [SkippableFact]
    public async Task Without_AClientId_SignIn_IsNotConfigured_AndTheApiStaysClosed()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        await using var host = api.WithWebHostBuilder(builder => builder.UseSetting("Identity:Google:ClientId", ""));
        using var client = host.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/identity/sign-in", new { credential = FakeGoogleVerifier.Credential(ApiFactory.Vitaliy) });

        await AssertProblemAsync(response, HttpStatusCode.Conflict, "identity.not_configured");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/expenses")).StatusCode);
    }

    [SkippableFact]
    public async Task A_SignIn_DeletesTheMembersLongEndedSessions()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        var now = DateTimeOffset.UtcNow;
        var stale = await StoreSessionAsync(ApiFactory.Masha, now.AddDays(-70));
        var recent = await StoreSessionAsync(ApiFactory.Masha, now.AddDays(-35));

        await api.SignInAsync(ApiFactory.Masha);

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        Assert.False(await db.Sessions.AnyAsync(s => s.TokenHash == SessionTokens.Hash(stale)));
        Assert.True(await db.Sessions.AnyAsync(s => s.TokenHash == SessionTokens.Hash(recent)));
    }

    private HttpClient WithToken(string token)
    {
        var client = api.CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<string> StoreSessionAsync(string email, DateTimeOffset startedAt)
    {
        var token = SessionTokens.New();
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.Sessions.Add(Session.Start(SessionTokens.Hash(token), new MemberProfile(email, "Маша", null), startedAt));
        await db.SaveChangesAsync();
        return token;
    }

    private async Task<Session> FindSessionAsync(string token)
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        return await db.Sessions.AsNoTracking().SingleAsync(s => s.TokenHash == SessionTokens.Hash(token));
    }

    private async Task<int> CountSessionsAsync(string email)
    {
        await using var scope = api.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Sessions.CountAsync(s => s.Email == email);
    }

    private static async Task AssertProblemAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(code, problem.GetProperty("code").GetString());
    }
}

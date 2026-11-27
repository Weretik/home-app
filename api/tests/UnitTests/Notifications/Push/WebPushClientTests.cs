using System.Net;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Notifications.Application;
using Notifications.Application.Push;
using Notifications.Infrastructure.Push;

namespace UnitTests.Notifications.Push;

/// <summary>The sender's request and how it reads the push services' answers (spec notifications/002, R-108).</summary>
public sealed class WebPushClientTests
{
    private static readonly PushPayload Payload = new("Дім", "Тест", "/todos", "test");

    [Fact]
    public async Task A_Message_IsPostedEncrypted_WithVapidAndTtl()
    {
        var handler = new StubHandler(HttpStatusCode.Created);

        var results = await Create(handler).SendAsync([Device()], Payload, CancellationToken.None);

        Assert.Equal(PushOutcome.Delivered, Assert.Single(results).Outcome);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("aes128gcm", Assert.Single(request.ContentEncoding));
        Assert.Equal("43200", request.Ttl);
        Assert.StartsWith("vapid t=", request.Authorization, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(HttpStatusCode.Gone, PushOutcome.Gone)]
    [InlineData(HttpStatusCode.NotFound, PushOutcome.Gone)]
    [InlineData(HttpStatusCode.TooManyRequests, PushOutcome.Failed)]
    public async Task The_Answer_SaysWhatBecameOfTheDevice(HttpStatusCode status, PushOutcome outcome)
    {
        var results = await Create(new StubHandler(status)).TrySendAsync([Device()], Payload, CancellationToken.None);

        Assert.Equal(outcome, Assert.Single(results).Outcome);
    }

    [Fact]
    public async Task Every_Device_Failing_Throws_SoTheOutboxRetries_ButAGoneOneDoesNot()
    {
        var failing = Create(new StubHandler(HttpStatusCode.InternalServerError));
        var gone = Create(new StubHandler(HttpStatusCode.Gone));

        await Assert.ThrowsAsync<HttpRequestException>(() => failing.SendAsync([Device(), Device()], Payload, CancellationToken.None));
        Assert.Equal(PushOutcome.Gone, Assert.Single(await gone.SendAsync([Device()], Payload, CancellationToken.None)).Outcome);
    }

    [Fact]
    public void Without_Both_Keys_PushIsNotConfigured()
    {
        var client = new WebPushClient(
            new HttpClient(new StubHandler(HttpStatusCode.Created)),
            Options.Create(new WebPushOptions { PublicKey = "x" }),
            new NotificationsSettings("https://web.example"),
            new FakeTimeProvider(),
            NullLogger<WebPushClient>.Instance);

        Assert.False(client.IsConfigured);
        Assert.Null(client.PublicKey);
    }

    private static PushTarget Device()
    {
        using var browser = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        return new PushTarget(
            Guid.NewGuid(),
            "https://push.example/send/abc",
            Base64Url.Encode(PushEncryption.PublicPoint(browser)),
            Base64Url.Encode(RandomNumberGenerator.GetBytes(16)));
    }

    private static WebPushClient Create(StubHandler handler)
    {
        using var vapid = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = vapid.ExportParameters(true);
        var options = new WebPushOptions { PublicKey = Base64Url.Encode([4, .. p.Q.X!, .. p.Q.Y!]), PrivateKey = Base64Url.Encode(p.D) };
        return new WebPushClient(
            new HttpClient(handler),
            Options.Create(options),
            new NotificationsSettings("https://web.example"),
            new FakeTimeProvider(new DateTimeOffset(2026, 11, 27, 8, 0, 0, TimeSpan.Zero)),
            NullLogger<WebPushClient>.Instance);
    }

    private sealed record SeenRequest(HttpMethod Method, ICollection<string> ContentEncoding, string? Ttl, string? Authorization);

    private sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public List<SeenRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new SeenRequest(
                request.Method,
                request.Content!.Headers.ContentEncoding,
                request.Headers.TryGetValues("TTL", out var ttl) ? ttl.Single() : null,
                request.Headers.TryGetValues("Authorization", out var auth) ? auth.Single() : null));
            return Task.FromResult(new HttpResponseMessage(status));
        }
    }
}

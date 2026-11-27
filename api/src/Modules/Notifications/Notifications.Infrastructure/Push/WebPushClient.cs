using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.Application;
using Notifications.Application.Push;

namespace Notifications.Infrastructure.Push;

/// <summary>
/// <see cref="IWebPush"/> over the standard protocol (RFC 8030): each message is encrypted for its device
/// (<see cref="PushEncryption"/>) and posted to the device's push service with a VAPID token. Logs name the push
/// service host only: the rest of an endpoint addresses the device.
/// </summary>
internal sealed partial class WebPushClient(
    HttpClient http,
    IOptions<WebPushOptions> options,
    NotificationsSettings site,
    TimeProvider clock,
    ILogger<WebPushClient> logger) : IWebPush
{
    /// <summary>How long a push service keeps a message for a device that is offline: a digest is stale after half a day.</summary>
    public const int TimeToLiveSeconds = 43_200;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public bool IsConfigured => options.Value.IsConfigured;

    public string? PublicKey => IsConfigured ? options.Value.PublicKey.Trim().TrimEnd('=') : null;

    public async Task<IReadOnlyList<PushResult>> SendAsync(
        IReadOnlyList<PushTarget> devices, PushPayload payload, CancellationToken cancellationToken)
    {
        var results = await TrySendAsync(devices, payload, cancellationToken);
        if (results.Count > 0 && results.All(r => r.Outcome == PushOutcome.Failed))
            throw new HttpRequestException($"No push service accepted the message ({results.Count} device(s)).");
        return results;
    }

    public async Task<IReadOnlyList<PushResult>> TrySendAsync(
        IReadOnlyList<PushTarget> devices, PushPayload payload, CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(payload, Json);
        var results = new List<PushResult>(devices.Count);
        foreach (var device in devices)
            results.Add(new PushResult(device.SubscriptionId, await SendOneAsync(device, body, cancellationToken)));
        return results;
    }

    private async Task<PushOutcome> SendOneAsync(PushTarget device, byte[] body, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            return PushOutcome.Failed;
        if (!Uri.TryCreate(device.Endpoint, UriKind.Absolute, out var endpoint))
            return PushOutcome.Gone;

        try
        {
            using var request = Request(endpoint, device, body);
            using var response = await http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return PushOutcome.Delivered;
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
                return PushOutcome.Gone;
            LogRefused(logger, endpoint.Host, (int)response.StatusCode);
            return PushOutcome.Failed;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or CryptographicException or FormatException or ArgumentException)
        {
            LogFailed(logger, endpoint.Host, exception.GetType().Name);
            return PushOutcome.Failed;
        }
    }

    private HttpRequestMessage Request(Uri endpoint, PushTarget device, byte[] body)
    {
        var settings = options.Value;
        var encrypted = PushEncryption.Encrypt(body, Base64Url.Decode(device.P256dh), Base64Url.Decode(device.Auth));
        var subject = string.IsNullOrWhiteSpace(settings.Subject) ? site.SiteUrl : settings.Subject;
        var content = new ByteArrayContent(encrypted);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentEncoding.Add("aes128gcm");
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };
        request.Headers.TryAddWithoutValidation("TTL", TimeToLiveSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        request.Headers.TryAddWithoutValidation("Urgency", "normal");
        request.Headers.TryAddWithoutValidation(
            "Authorization", VapidToken.Authorization(endpoint, subject, settings.PublicKey, settings.PrivateKey, clock.GetUtcNow()));
        return request;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The push service {Host} refused a message with {Status}")]
    private static partial void LogRefused(ILogger logger, string host, int status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "A push to {Host} failed: {Error}")]
    private static partial void LogFailed(ILogger logger, string host, string error);
}

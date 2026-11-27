using BuildingBlocks.Domain.Modeling;
using Notifications.Domain.Chats;

namespace Notifications.Domain.Push;

/// <summary>
/// A browser's Web Push subscription for one person (spec notifications/002, R-101, R-102): the push service endpoint
/// and the keys the payload is encrypted for. The endpoint is unique, so saving it again refreshes the same row; a
/// device signed in by the other person moves to that person.
/// </summary>
public sealed class PushSubscription : Entity<PushSubscriptionId>, IAggregateRoot
{
    public const int MaxEndpointLength = 1000;
    public const int MaxP256dhLength = 200;
    public const int MaxAuthLength = 100;
    public const int MaxDeviceLength = 120;

    private PushSubscription()
    {
    }

    /// <summary><see cref="Audience.Vitaliy"/> or <see cref="Audience.Maria"/>, never the family group.</summary>
    public Audience Person { get; private set; }

    public string Endpoint { get; private set; } = string.Empty;

    /// <summary>The browser's P-256 public key, base64url (65 bytes uncompressed).</summary>
    public string P256dh { get; private set; } = string.Empty;

    /// <summary>The browser's 16-byte authentication secret, base64url.</summary>
    public string Auth { get; private set; } = string.Empty;

    /// <summary>What the page called the device («Android · Chrome»), cut to <see cref="MaxDeviceLength"/>.</summary>
    public string? Device { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public DateTimeOffset LastSeenAtUtc { get; private set; }

    public static PushSubscription Save(
        Audience person, string endpoint, string p256dh, string auth, string? device, DateTimeOffset now)
    {
        var subscription = new PushSubscription
        {
            Id = PushSubscriptionId.New(now),
            Endpoint = endpoint,
            CreatedAtUtc = now,
        };
        subscription.Refresh(person, p256dh, auth, device, now);
        return subscription;
    }

    /// <summary>
    /// The same endpoint saved again: new keys, owner and label, seen now (R-102). The person comes from the session,
    /// which only knows Vitaliy and Maria.
    /// </summary>
    public void Refresh(Audience person, string p256dh, string auth, string? device, DateTimeOffset now)
    {
        Person = person;
        P256dh = p256dh;
        Auth = auth;
        Device = Shorten(device);
        LastSeenAtUtc = now;
    }

    private static string? Shorten(string? device)
    {
        var trimmed = device?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= MaxDeviceLength ? trimmed : trimmed[..MaxDeviceLength];
    }
}

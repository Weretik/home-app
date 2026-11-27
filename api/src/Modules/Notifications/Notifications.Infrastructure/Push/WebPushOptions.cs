namespace Notifications.Infrastructure.Push;

/// <summary>
/// The VAPID key pair (ADR 0042): <c>WebPush:PublicKey</c> (65-byte P-256 point, base64url) and
/// <c>WebPush:PrivateKey</c> (32 bytes, base64url, secret; App Service <c>WebPush__PrivateKey</c>), plus the contact
/// <c>WebPush:Subject</c> (a <c>mailto:</c> or <c>https:</c> URL; the site URL when empty). Push is configured only when
/// both keys are set.
/// </summary>
public sealed class WebPushOptions
{
    public const string SectionName = "WebPush";

    public string PublicKey { get; set; } = string.Empty;

    public string PrivateKey { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(PublicKey) && !string.IsNullOrWhiteSpace(PrivateKey);
}

namespace Notifications.Application.Push;

/// <summary>One device to push to: its subscription id, push service endpoint and browser keys (base64url).</summary>
public sealed record PushTarget(Guid SubscriptionId, string Endpoint, string P256dh, string Auth);

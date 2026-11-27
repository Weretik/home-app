namespace Notifications.Api.Endpoints.Push;

/// <summary>A browser's push subscription (<c>PushSubscription.toJSON()</c>: endpoint and keys) and a device label.</summary>
public sealed record SavePushSubscriptionRequest(string Endpoint, string P256dh, string Auth, string? Device);

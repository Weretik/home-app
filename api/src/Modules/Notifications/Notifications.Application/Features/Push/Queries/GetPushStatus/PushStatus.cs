namespace Notifications.Application.Features.Push.Queries.GetPushStatus;

/// <summary>
/// Push for the signed-in person: <see cref="PublicKey"/> (base64url) is what the browser subscribes with, null while
/// push is not configured; <see cref="CanSubscribe"/> is false for a member without a person (R-101).
/// </summary>
public sealed record PushStatus(bool Configured, string? PublicKey, bool CanSubscribe, IReadOnlyList<PushDevice> Devices);

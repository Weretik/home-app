namespace Notifications.Application.Push;

/// <summary>
/// Standard Web Push with VAPID as the module needs it (ADR 0042); Infrastructure encrypts and posts to the browsers'
/// push services. Configured when both VAPID keys are set (R-104).
/// </summary>
public interface IWebPush
{
    bool IsConfigured { get; }

    /// <summary>The VAPID public key (base64url) the browser subscribes with; null when not configured.</summary>
    string? PublicKey { get; }

    /// <summary>
    /// Sends <paramref name="payload"/> to every device and says what became of each. Throws when no device accepted
    /// it and none is gone, so the outbox tries again later (R-108); never throws for a single bad device otherwise.
    /// </summary>
    Task<IReadOnlyList<PushResult>> SendAsync(IReadOnlyList<PushTarget> devices, PushPayload payload, CancellationToken cancellationToken);

    /// <summary>Sends at once, best effort (the test push): what became of each device, never throws.</summary>
    Task<IReadOnlyList<PushResult>> TrySendAsync(IReadOnlyList<PushTarget> devices, PushPayload payload, CancellationToken cancellationToken);
}

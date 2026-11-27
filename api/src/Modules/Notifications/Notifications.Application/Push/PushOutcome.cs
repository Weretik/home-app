namespace Notifications.Application.Push;

/// <summary>A push service's answer for one device.</summary>
public enum PushOutcome
{
    /// <summary>Accepted (201, 200 or 202).</summary>
    Delivered,

    /// <summary>The subscription no longer exists (404, 410): delete it.</summary>
    Gone,

    /// <summary>Any other answer or a network error: try again later.</summary>
    Failed,
}

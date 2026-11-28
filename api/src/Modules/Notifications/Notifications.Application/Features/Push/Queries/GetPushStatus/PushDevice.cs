using Notifications.Domain.Push;

namespace Notifications.Application.Features.Push.Queries.GetPushStatus;

/// <summary>One of the person's devices; the endpoint lets the page recognise the device it runs on.</summary>
public sealed record PushDevice(Guid Id, string? Device, string Endpoint, DateTimeOffset CreatedAtUtc, DateTimeOffset LastSeenAtUtc)
{
    public static PushDevice Of(PushSubscription s) => new(s.Id.Value, s.Device, s.Endpoint, s.CreatedAtUtc, s.LastSeenAtUtc);
}

using System.Collections.Concurrent;
using Notifications.Application.Push;

namespace IntegrationTests.Api.Notifications.Push;

/// <summary>Web Push for the tests: records each push; an endpoint containing «gone» answers as a removed subscription.</summary>
internal sealed class FakeWebPush : IWebPush
{
    public ConcurrentQueue<(string Endpoint, PushPayload Payload)> Sent { get; } = new();

    public bool IsConfigured => true;

    public string? PublicKey => "BFakePublicKeyForTheIntegrationTests";

    public Task<IReadOnlyList<PushResult>> SendAsync(IReadOnlyList<PushTarget> devices, PushPayload payload, CancellationToken cancellationToken) =>
        TrySendAsync(devices, payload, cancellationToken);

    public Task<IReadOnlyList<PushResult>> TrySendAsync(IReadOnlyList<PushTarget> devices, PushPayload payload, CancellationToken cancellationToken)
    {
        var results = new List<PushResult>();
        foreach (var device in devices)
        {
            var gone = device.Endpoint.Contains("gone", StringComparison.Ordinal);
            if (!gone)
                Sent.Enqueue((device.Endpoint, payload));
            results.Add(new PushResult(device.SubscriptionId, gone ? PushOutcome.Gone : PushOutcome.Delivered));
        }

        return Task.FromResult<IReadOnlyList<PushResult>>(results);
    }
}

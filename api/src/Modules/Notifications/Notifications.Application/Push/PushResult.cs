namespace Notifications.Application.Push;

/// <summary>What became of one device: <see cref="PushOutcome.Gone"/> devices are deleted (R-108).</summary>
public sealed record PushResult(Guid SubscriptionId, PushOutcome Outcome);

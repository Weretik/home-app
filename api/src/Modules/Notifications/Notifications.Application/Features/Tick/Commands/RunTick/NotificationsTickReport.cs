namespace Notifications.Application.Features.Tick.Commands.RunTick;

/// <summary>What one tick did: whether the bot is configured, the webhook state and how many messages it planned.</summary>
public sealed record NotificationsTickReport(bool Configured, string Webhook, int Planned);

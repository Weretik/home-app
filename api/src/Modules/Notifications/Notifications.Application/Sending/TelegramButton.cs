namespace Notifications.Application.Sending;

/// <summary>An inline button: what it says and the callback data it sends back (at most 64 bytes).</summary>
public sealed record TelegramButton(string Text, string Data);

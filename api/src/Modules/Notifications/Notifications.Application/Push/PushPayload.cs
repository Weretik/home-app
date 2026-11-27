namespace Notifications.Application.Push;

/// <summary>
/// What the service worker shows (spec notifications/002, R-107): a title, plain text, the page a tap opens and a tag,
/// so a newer message of the same kind replaces the older one. Sent as JSON <c>{ title, body, url, tag }</c>.
/// </summary>
public sealed record PushPayload(string Title, string Body, string Url, string Tag);

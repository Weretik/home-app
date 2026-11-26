using Notifications.Domain.Deliveries;

namespace Notifications.Domain.Schedule;

/// <summary>One line of the schedule as the site shows it: the kind, what it is and when it comes (Russian).</summary>
public sealed record DigestWindow(DigestKind Kind, string Title, string When);

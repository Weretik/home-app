using Notifications.Domain.Deliveries;

namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>One line of the schedule: the kind, what it is and when it comes (Ukrainian, Kyiv time).</summary>
public sealed record NotificationScheduleItem(DigestKind Kind, string Title, string When);

using Notifications.Domain.Deliveries;

namespace Notifications.Domain.Schedule;

/// <summary>
/// A digest that is due now: its kind, the period it covers (<see cref="From"/> to <see cref="To"/>, both included)
/// and the period's key, such as <c>2026-11-27</c>, <c>2026-11-23</c> (a week by its Monday) or <c>2026-11</c>.
/// </summary>
public sealed record DueSlot(DigestKind Kind, string Period, DateOnly From, DateOnly To);

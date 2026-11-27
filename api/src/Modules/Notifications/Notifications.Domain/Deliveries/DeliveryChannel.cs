namespace Notifications.Domain.Deliveries;

/// <summary>How a planned message reaches its audience (spec notifications/002, R-106).</summary>
public enum DeliveryChannel
{
    Telegram,
    Push,
}

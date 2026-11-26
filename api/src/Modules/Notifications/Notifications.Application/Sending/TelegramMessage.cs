namespace Notifications.Application.Sending;

/// <summary>Outbox message: send <see cref="Text"/> to <see cref="ChatId"/> for the delivery <see cref="DeliveryId"/>.</summary>
public sealed record TelegramMessage(Guid DeliveryId, long ChatId, string Text, IReadOnlyList<TelegramButton> Buttons)
{
    public const string TypeName = "notifications.telegram_message";
}

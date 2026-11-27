using Notifications.Application.Push;
using Notifications.Domain.Chats;

namespace Notifications.Application.Sending;

/// <summary>Outbox message: push <see cref="Payload"/> to every device of <see cref="Person"/> for the delivery <see cref="DeliveryId"/>.</summary>
public sealed record PushMessage(Guid DeliveryId, Audience Person, PushPayload Payload)
{
    public const string TypeName = "notifications.push_message";
}

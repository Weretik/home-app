using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Push;
using Notifications.Domain.Deliveries;
using Notifications.Domain.Push;

namespace Notifications.Application.Sending;

/// <summary>
/// Pushes a planned message to every device of the person (spec notifications/002, R-108): devices the push service
/// reports gone are deleted, and the delivery counts as sent when a device accepted it or none is left. When no device
/// accepted it and none is gone, <see cref="IWebPush.SendAsync"/> throws and the outbox retries with growing pauses.
/// </summary>
public sealed class PushMessageHandler(INotificationsDbContext db, IWebPush push, TimeProvider clock)
    : IOutboxMessageHandler<PushMessage>
{
    public async Task HandleAsync(PushMessage message, CancellationToken cancellationToken)
    {
        var devices = await db.PushSubscriptions.AsNoTracking()
            .Where(s => s.Person == message.Person)
            .Select(s => new PushTarget(s.Id.Value, s.Endpoint, s.P256dh, s.Auth))
            .ToListAsync(cancellationToken);

        if (devices.Count > 0)
        {
            var results = await push.SendAsync(devices, message.Payload, cancellationToken);
            var gone = results.Where(r => r.Outcome == PushOutcome.Gone).Select(r => new PushSubscriptionId(r.SubscriptionId)).ToList();
            if (gone.Count > 0)
                await db.PushSubscriptions.Where(s => gone.Contains(s.Id)).ExecuteDeleteAsync(cancellationToken);
        }

        var now = clock.GetUtcNow();
        await db.Deliveries
            .Where(d => d.Id == new DeliveryId(message.DeliveryId))
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.SentAtUtc, (DateTimeOffset?)now), cancellationToken);
    }
}

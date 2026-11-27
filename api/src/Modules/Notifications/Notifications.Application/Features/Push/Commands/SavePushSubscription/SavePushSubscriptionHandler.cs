using BuildingBlocks.Domain.Results;
using Identity.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Features.Push.Queries.GetPushStatus;
using Notifications.Application.Push;
using Notifications.Domain.Push;

namespace Notifications.Application.Features.Push.Commands.SavePushSubscription;

/// <summary>
/// Upsert by endpoint (R-102): a known endpoint gets new keys, label and owner and is seen now; a new one is added. A
/// repeat is harmless, so no Idempotency-Key. Two first saves of one endpoint at the same moment: the unique index
/// refuses the second, and the page saves again on its next start.
/// </summary>
public sealed class SavePushSubscriptionHandler(INotificationsDbContext db, IWebPush push, ICurrentMember member, TimeProvider clock)
    : ICommandHandler<SavePushSubscriptionCommand, Result<PushDevice>>
{
    public async ValueTask<Result<PushDevice>> Handle(SavePushSubscriptionCommand c, CancellationToken cancellationToken)
    {
        if (!push.IsConfigured)
            return PushErrors.NotConfigured;
        if (PushPeople.Of(member) is not { } person)
            return PushErrors.NoPerson;

        var now = clock.GetUtcNow();
        var endpoint = c.Endpoint.Trim();
        var subscription = await db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint, cancellationToken);
        if (subscription is null)
        {
            subscription = PushSubscription.Save(person, endpoint, c.P256dh.Trim(), c.Auth.Trim(), c.Device, now);
            db.PushSubscriptions.Add(subscription);
        }
        else
        {
            subscription.Refresh(person, c.P256dh.Trim(), c.Auth.Trim(), c.Device, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return PushDevice.Of(subscription);
    }
}

using BuildingBlocks.Domain.Results;
using Identity.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Push;
using Notifications.Domain.Push;

namespace Notifications.Application.Features.Push.Commands.SendPushTest;

/// <summary>
/// Sends at once, not through the outbox: the person waits for the answer (R-109). Devices the push service reports
/// gone are deleted, as the outbox does (R-108).
/// </summary>
public sealed class SendPushTestHandler(INotificationsDbContext db, IWebPush push, ICurrentMember member)
    : ICommandHandler<SendPushTestCommand, Result<PushTestResult>>
{
    public static readonly PushPayload Payload = new("Дім", "✅ Push-сповіщення працюють на цьому пристрої.", "/notifications", "test");

    public async ValueTask<Result<PushTestResult>> Handle(SendPushTestCommand command, CancellationToken cancellationToken)
    {
        if (!push.IsConfigured)
            return PushErrors.NotConfigured;
        if (PushPeople.Of(member) is not { } person)
            return PushErrors.NoPerson;

        var devices = await db.PushSubscriptions.AsNoTracking()
            .Where(s => s.Person == person)
            .Select(s => new PushTarget(s.Id.Value, s.Endpoint, s.P256dh, s.Auth))
            .ToListAsync(cancellationToken);
        if (devices.Count == 0)
            return PushErrors.NoDevices;

        var results = await push.TrySendAsync(devices, Payload, cancellationToken);
        var gone = results.Where(r => r.Outcome == PushOutcome.Gone).Select(r => new PushSubscriptionId(r.SubscriptionId)).ToList();
        if (gone.Count > 0)
            await db.PushSubscriptions.Where(s => gone.Contains(s.Id)).ExecuteDeleteAsync(cancellationToken);

        var delivered = results.Count(r => r.Outcome == PushOutcome.Delivered);
        return delivered > 0 ? new PushTestResult(delivered) : PushErrors.Failed;
    }
}

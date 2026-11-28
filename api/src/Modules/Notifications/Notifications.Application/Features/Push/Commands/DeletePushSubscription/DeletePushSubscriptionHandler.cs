using BuildingBlocks.Domain.Results;
using Identity.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Push;
using Notifications.Domain.Push;

namespace Notifications.Application.Features.Push.Commands.DeletePushSubscription;

/// <summary>Another person's device or a missing one answers 404 alike, so nobody learns the other's devices (R-103).</summary>
public sealed class DeletePushSubscriptionHandler(INotificationsDbContext db, ICurrentMember member)
    : ICommandHandler<DeletePushSubscriptionCommand, Result>
{
    public async ValueTask<Result> Handle(DeletePushSubscriptionCommand c, CancellationToken cancellationToken)
    {
        var person = PushPeople.Of(member);
        var subscription = person is null
            ? null
            : await db.PushSubscriptions.FirstOrDefaultAsync(s => s.Id == new PushSubscriptionId(c.Id) && s.Person == person, cancellationToken);
        if (subscription is null)
            return PushErrors.DeviceNotFound;

        db.PushSubscriptions.Remove(subscription);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

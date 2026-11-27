using BuildingBlocks.Domain.Results;
using Identity.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Push;

namespace Notifications.Application.Features.Push.Queries.GetPushStatus;

/// <summary>Only the signed-in person's devices, newest seen first; none for the scheduler or a member without a person.</summary>
public sealed class GetPushStatusHandler(INotificationsDbContext db, IWebPush push, ICurrentMember member)
    : IQueryHandler<GetPushStatusQuery, Result<PushStatus>>
{
    public async ValueTask<Result<PushStatus>> Handle(GetPushStatusQuery query, CancellationToken cancellationToken)
    {
        var person = PushPeople.Of(member);
        var devices = person is null
            ? []
            : await db.PushSubscriptions.AsNoTracking()
                .Where(s => s.Person == person)
                .OrderByDescending(s => s.LastSeenAtUtc)
                .Select(s => new PushDevice(s.Id, s.Device, s.Endpoint, s.CreatedAtUtc, s.LastSeenAtUtc))
                .ToListAsync(cancellationToken);
        return new PushStatus(push.IsConfigured, push.PublicKey, person is not null, devices);
    }
}

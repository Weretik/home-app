using BuildingBlocks.Domain.Results;
using Mediator;
using Notifications.Application.Features.Push.Queries.GetPushStatus;

namespace Notifications.Application.Features.Push.Commands.SavePushSubscription;

/// <summary>Saves this browser's push subscription for the signed-in person, an upsert by endpoint (spec notifications/002, R-102).</summary>
public sealed record SavePushSubscriptionCommand(string Endpoint, string P256dh, string Auth, string? Device)
    : ICommand<Result<PushDevice>>;

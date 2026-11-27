using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Push.Commands.DeletePushSubscription;

/// <summary>Removes one of the signed-in person's devices (spec notifications/002, R-103).</summary>
public sealed record DeletePushSubscriptionCommand(Guid Id) : ICommand<Result>;

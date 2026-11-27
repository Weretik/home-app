using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Push.Queries.GetPushStatus;

/// <summary>Whether push is configured, its public key and the signed-in person's devices (spec notifications/002, R-103, R-104).</summary>
public sealed record GetPushStatusQuery : IQuery<Result<PushStatus>>;

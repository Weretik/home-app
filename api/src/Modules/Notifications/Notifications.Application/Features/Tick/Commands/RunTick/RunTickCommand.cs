using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Tick.Commands.RunTick;

/// <summary>
/// One scheduler call (every 15 minutes): make sure the webhook is set, then plan every digest that is due now and
/// not planned yet (spec notifications/001, R-006, R-007, R-017).
/// </summary>
public sealed record RunTickCommand : ICommand<Result<NotificationsTickReport>>;

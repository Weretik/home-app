using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Push.Commands.SendPushTest;

/// <summary>Pushes a test message to every device of the signed-in person at once (spec notifications/002, R-109).</summary>
public sealed record SendPushTestCommand : ICommand<Result<PushTestResult>>;

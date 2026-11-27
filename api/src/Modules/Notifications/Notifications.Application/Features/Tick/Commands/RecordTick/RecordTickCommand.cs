using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Tick.Commands.RecordTick;

/// <summary>
/// Remembers that the scheduler called, before the tick plans anything, so the «Телеграм» page can tell a silent
/// scheduler from a quiet day (spec notifications/001, R-020).
/// </summary>
public sealed record RecordTickCommand : ICommand<Result>;

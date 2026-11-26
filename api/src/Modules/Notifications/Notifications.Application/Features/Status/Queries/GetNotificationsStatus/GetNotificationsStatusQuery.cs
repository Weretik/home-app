using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notifications.Application.Features.Status.Queries.GetNotificationsStatus;

/// <summary>The bot as the site's «Телеграм» page shows it.</summary>
public sealed record GetNotificationsStatusQuery : IQuery<Result<NotificationsStatus>>;

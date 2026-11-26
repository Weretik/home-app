using Notifications.Domain.Chats;

namespace Notifications.Api.Endpoints.Site;

/// <summary>Which linked chat gets the test message.</summary>
internal sealed record NotificationsTestRequest(Audience Audience);

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notifications.Application.Features.Chats.Commands.SendTestMessage;

namespace Notifications.Api.Endpoints.Site;

internal static class SendNotificationsTestEndpoint
{
    public static RouteGroupBuilder MapSendNotificationsTest(this RouteGroupBuilder group)
    {
        group.MapPost("/test", HandleAsync)
            .WithName("sendNotificationsTest")
            .WithSummary("Sends «✅ Бот на связи» to a linked chat at once. 404 when the chat is not linked, 409 when the bot token is not set or Telegram refused.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        NotificationsTestRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SendTestMessageCommand(request.Audience), cancellationToken);
        return result.ToHttpResult(http);
    }
}

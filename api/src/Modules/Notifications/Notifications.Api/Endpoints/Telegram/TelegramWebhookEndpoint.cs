using System.Text.Json;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Notifications.Infrastructure.Telegram;

namespace Notifications.Api.Endpoints.Telegram;

/// <summary>
/// <c>POST /api/notifications/telegram/webhook</c>: Telegram delivers the bot's updates here (ADR 0033). Only calls
/// with the secret header derived from the token get in; every accepted call answers 200, so Telegram never resends
/// an update the bot has seen (R-016). Hidden from OpenAPI; no rate limit, since Telegram is the only caller.
/// </summary>
internal static class TelegramWebhookEndpoint
{
    public static IEndpointRouteBuilder MapTelegramWebhook(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(TelegramOptions.WebhookPath, HandleAsync).ExcludeFromDescription();
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext http, IOptions<TelegramOptions> options, ISender sender, CancellationToken cancellationToken)
    {
        var token = options.Value.BotToken;
        if (string.IsNullOrWhiteSpace(token))
            return TypedResults.NotFound();
        if (!TelegramWebhookSecret.Matches(token, http.Request.Headers[TelegramWebhookSecret.HeaderName].ToString()))
            return TypedResults.Unauthorized();

        JsonDocument update;
        try
        {
            update = await JsonDocument.ParseAsync(http.Request.Body, cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            return TypedResults.Ok();
        }

        using (update)
        {
            if (TelegramUpdateParser.Parse(update.RootElement) is { } command)
                await sender.Send(command, cancellationToken);
        }

        return TypedResults.Ok();
    }
}

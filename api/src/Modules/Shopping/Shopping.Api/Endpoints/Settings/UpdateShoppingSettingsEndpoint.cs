using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Settings.Commands.UpdateSettings;

namespace Shopping.Api.Endpoints.Settings;

internal static class UpdateShoppingSettingsEndpoint
{
    public static RouteGroupBuilder MapUpdateShoppingSettings(this RouteGroupBuilder group)
    {
        group.MapPut("/settings", HandleAsync)
            .WithName("updateShoppingSettings")
            .WithSummary("Sets how long bought items stay in the cart (1–72 hours) and the frequent window (30–730 days). Send the version in If-Match.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = VersionHeader.Name)] string? ifMatch,
        Request request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSettingsCommand(VersionHeader.Parse(ifMatch), request.CartKeepHours, request.FrequentDays), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The new settings.</summary>
    internal sealed record Request(int CartKeepHours, int FrequentDays);
}

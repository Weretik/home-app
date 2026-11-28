using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Identity.Api.Authentication;
using Identity.Application.Features.Sessions.Commands.SignOutEverywhere;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints;

/// <summary><c>POST /api/identity/sign-out-everywhere</c>: ends every session of the signed-in member (ADR 0046).</summary>
internal static class SignOutEverywhereEndpoint
{
    public static RouteGroupBuilder MapSignOutEverywhere(this RouteGroupBuilder group)
    {
        group.MapPost("/sign-out-everywhere", HandleAsync)
            .WithName("signOutEverywhere")
            .WithSummary("Ends every session of the signed-in member on every device, this one included.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var token = MemberClaims.BearerToken(http.Request) ?? string.Empty;
        var result = await sender.Send(new SignOutEverywhereCommand(token), cancellationToken);
        return result.ToHttpResult(http);
    }
}

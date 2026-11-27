using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Identity.Api.Authentication;
using Identity.Application.Features.Sessions.Commands.SignOut;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints;

/// <summary><c>POST /api/identity/sign-out</c>: ends the session of the request's own token.</summary>
internal static class SignOutEndpoint
{
    public static RouteGroupBuilder MapSignOut(this RouteGroupBuilder group)
    {
        group.MapPost("/sign-out", HandleAsync)
            .WithName("signOut")
            .WithSummary("Ends the current session; the token stops working at once. Other devices stay signed in.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var token = MemberClaims.BearerToken(http.Request) ?? string.Empty;
        var result = await sender.Send(new SignOutCommand(token), cancellationToken);
        return result.ToHttpResult(http);
    }
}

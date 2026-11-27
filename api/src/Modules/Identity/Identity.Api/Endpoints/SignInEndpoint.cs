using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Identity.Application.Features.Sessions.Commands.SignIn;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints;

/// <summary><c>POST /api/identity/sign-in</c>: the one way in, so it is anonymous and slowed by the sign-in limit.</summary>
internal static class SignInEndpoint
{
    public static RouteGroupBuilder MapSignIn(this RouteGroupBuilder group)
    {
        group.MapPost("/sign-in", HandleAsync)
            .WithName("signIn")
            .WithSummary("Signs a family member in with a Google ID token and returns a session token. 401 when Google did not confirm the token, 403 when the e-mail is not on the family list, 409 when sign-in is not configured.")
            .Produces<IdentitySession>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.SignIn)
            .AllowAnonymous();
        return group;
    }

    private static async Task<IResult> HandleAsync(Request request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SignInCommand(request.Credential), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The ID token (<c>credential</c>) that Google Identity Services gave the web app.</summary>
    internal sealed record Request(string Credential);
}

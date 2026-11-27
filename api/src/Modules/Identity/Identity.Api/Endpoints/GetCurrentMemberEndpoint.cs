using Identity.Api.Authentication;
using Identity.Application.Features.Members;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints;

/// <summary><c>GET /api/identity/me</c>: who the session belongs to, from the claims the session handler set.</summary>
internal static class GetCurrentMemberEndpoint
{
    public static RouteGroupBuilder MapGetCurrentMember(this RouteGroupBuilder group)
    {
        group.MapGet("/me", Handle)
            .WithName("getCurrentMember")
            .WithSummary("Returns the signed-in family member: e-mail, name and Google picture.")
            .Produces<FamilyMember>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        return group;
    }

    private static IResult Handle(HttpContext http) =>
        MemberClaims.Member(http.User) is { } member ? TypedResults.Ok(member) : TypedResults.Unauthorized();
}

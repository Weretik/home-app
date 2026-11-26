using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Keywords.Commands.UpdateKeyword;

namespace Shopping.Api.Endpoints.Keywords;

internal static class UpdateShoppingKeywordEndpoint
{
    public static RouteGroupBuilder MapUpdateShoppingKeyword(this RouteGroupBuilder group)
    {
        group.MapPut("/keywords/{id:guid}", HandleAsync)
            .WithName("updateShoppingKeyword")
            .WithSummary("Changes a keyword's text or department. Send its version in If-Match.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        [FromHeader(Name = VersionHeader.Name)] string? ifMatch,
        Request request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateKeywordCommand(id, VersionHeader.Parse(ifMatch), request.Keyword, request.DepartmentId), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The keyword's new text and department.</summary>
    internal sealed record Request(string Keyword, Guid DepartmentId);
}
